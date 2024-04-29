#nullable enable

using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class AkiBackendCommunication : IDisposable
    {
        public const int DEFAULT_TIMEOUT_MS = 9999;

        public string ProfileId;

        public string RemoteEndPoint;

        private static AkiBackendCommunication? m_Instance;
        public static AkiBackendCommunication Instance
        {
            get
            {
                if (m_Instance == null || m_Instance.ProfileId == null || m_Instance.RemoteEndPoint == null)
                    m_Instance = new AkiBackendCommunication();

                return m_Instance;
            }
        }

        private readonly HttpClient _httpClient;

        protected ManualLogSource Logger;

        public WebSocketSharp.WebSocket? WebSocket { get; private set; }

        public long BytesSent = 0;
        public long BytesReceived = 0;
        public ushort Ping = 0;
        public ConcurrentQueue<int> ServerPingSmooth { get; } = new();

        public static int PING_LIMIT_HIGH { get; } = 125;
        public static int PING_LIMIT_MID { get; } = 100;
        public static bool IsLocal;
        public bool HighPingMode;

        private Task? _periodicallySendPingTask;

        protected AkiBackendCommunication(ManualLogSource? logger = null)
        {
            // disable SSL encryption
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Logger = logger ?? BepInEx.Logging.Logger.CreateLogSource("Request");

            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.Contains("-token="))
                {
                    ProfileId = arg.Replace("-token=", string.Empty);
                    break;
                }
            }
            if (ProfileId == null)
            {
                throw new ArgumentNullException("could not find valid profile id for AkiBackend");
            }

            RemoteEndPoint = StayInTarkovHelperConstants.GetBackendUrl();
            if (RemoteEndPoint == null)
            {
                throw new ArgumentNullException("could not find valid remote endpoint for AkiBackend");
            }
            RemoteEndPoint = RemoteEndPoint.TrimEnd('/');
            IsLocal = RemoteEndPoint.Contains("127.0.0.1") || RemoteEndPoint.Contains("localhost");

            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseCookies = false // do not use CookieContainer, let us manage cookies thru headers ourselves
            });
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"PHPSESSID={ProfileId}");
            _httpClient.DefaultRequestHeaders.Add("SessionId", ProfileId);
            _httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");
            _httpClient.MaxResponseContentBufferSize = long.MaxValue;
            _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, 60000);

            PeriodicallySendPing();
            SITGameServerClientDataProcessing.OnLatencyUpdated += OnLatencyUpdated;
            HighPingMode = PluginConfigSettings.Instance!.CoopSettings.ForceHighPingMode;
        }

        private SITGameComponent? _gameComp = null;
        private string? _profileId;

        public void WebSocketCreate(SITGameComponent gameComp, string profileId)
        {
            if (WebSocket != null)
            {
                throw new InvalidOperationException("in-raid lifecycle violation, WebSocket already exists");
            }

            _gameComp = gameComp;
            _profileId = profileId;

            var webSocketPort = PluginConfigSettings.Instance?.CoopSettings.SITWebSocketPort;
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{webSocketPort}/{_profileId}?";
            Logger.LogDebug($"WebSocketCreate: {wsUrl}");

            WebSocket = new WebSocketSharp.WebSocket(wsUrl)
            {
                WaitTime = TimeSpan.FromMinutes(1),
                EmitOnPing = true
            };
            WebSocket.Connect();

            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
        }

        public void WebSocketClose()
        {
            if (WebSocket != null)
            {
                Logger.LogDebug("WebSocketClose");
                WebSocket.OnError -= WebSocket_OnError;
                WebSocket.OnMessage -= WebSocket_OnMessage;
                WebSocket.Close(WebSocketSharp.CloseStatusCode.Normal);
                WebSocket = null;
            }

            _profileId = null;
            _gameComp = null;
        }

        public async void PingAsync()
        {
            await Task.Run(() =>
            {
                WebSocket?.Ping();
            });
        }

        public async void PostDownWebSocketImmediately(string packet)
        {
            await Task.Run(() =>
            {
                if (WebSocket != null)
                {
                    Interlocked.Add(ref BytesSent, Encoding.UTF8.GetByteCount(packet));
                    WebSocket.Send(packet);
                }
            });
        }

        public void PostDownWebSocketImmediately(byte[] packet)
        {
            if (WebSocket != null)
            {
                Interlocked.Add(ref BytesSent, packet.Length);
                WebSocket.SendAsync(packet, (b) => { });
            }
        }

        private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Logger.LogError($"{nameof(WebSocket_OnError)}: {e.Message} {e.Exception}");
            WebSocket_OnError();
            WebSocketClose();

            if (_gameComp == null || _profileId == null)
            {
                Logger.LogError($"unexpected in-raid lifecycle violation {_gameComp} {_profileId}");
                return;
            }

            WebSocketCreate(_gameComp, _profileId);
        }

        private void WebSocket_OnError()
        {
            Logger.LogError($"Your PC has failed to connect and send data to the WebSocket with the port {PluginConfigSettings.Instance?.CoopSettings.SITWebSocketPort} on the Server {StayInTarkovHelperConstants.GetBackendUrl()}! Application will now close.");
            if (Singleton<ISITGame>.Instantiated)
            {
                Singleton<ISITGame>.Instance.Stop(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, Singleton<ISITGame>.Instance.MyExitStatus, Singleton<ISITGame>.Instance.MyExitLocation);
            }
            else
            {
                Application.Quit();
            }
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (e == null)
                return;

            Interlocked.Add(ref BytesReceived, e.RawData.Length);

            var d = e.RawData;
            if (d.Length >= 3 && d[0] != '{' && !(d[0] == 'S' && d[1] == 'I' && d[2] == 'T'))
            {
                SITGameServerClientDataProcessing.ProcessFlatBuffer(_gameComp, d);
            }
            else
            {
                SITGameServerClientDataProcessing.ProcessPacketBytes(_gameComp, e.RawData);
            }
        }

        public static AkiBackendCommunication GetRequestInstance(bool createInstance = false, ManualLogSource logger = null)
        {
            if (createInstance)
            {
                return new AkiBackendCommunication(logger);
            }

            return Instance;
        }

        private void PeriodicallySendPing()
        {
            _periodicallySendPingTask = Task.Run(async () =>
            {
                int awaitPeriod = 2000;
                while (true)
                {
                    try
                    {
                        await Task.Delay(awaitPeriod);

                        if (WebSocket == null)
                            continue;

                        if (WebSocket.ReadyState != WebSocketSharp.WebSocketState.Open)
                            continue;

                        if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                            continue;

                        var packet = new
                        {
                            m = "Ping",
                            t = DateTime.UtcNow.Ticks.ToString("G"),
                            profileId = ProfileId,
                            serverId = coopGameComponent.ServerId
                        };

                        WebSocket.Send(Encoding.UTF8.GetBytes(packet.ToJson()));
                        packet = null;
                    } catch (Exception ex)
                    {
                        Logger.LogError($"Periodic ping caught: {ex.GetType()} {ex.Message}");
                    }
                }
            });
        }

        private void OnLatencyUpdated(ushort latencyMs)
        {
            if (ServerPingSmooth.Count > 5)
                ServerPingSmooth.TryDequeue(out _);
            ServerPingSmooth.Enqueue(latencyMs);
            Ping = (ushort)(ServerPingSmooth.Count > 0 ? Math.Round(ServerPingSmooth.Average()) : 1);
        }

        private async Task<byte[]?> AsyncRequestFromPath(string path, HttpMethod method, string? data = null, int timeout = 9999, bool debug = false)
        {
            if (!Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                path = RemoteEndPoint + path;
            }

            return await AsyncRequest(new Uri(path), method, data, timeout, debug);
        }

        private async Task<byte[]?> AsyncRequest(Uri uri, HttpMethod method, string? data = null, int timeout = 9999, bool debug = false)
        {
            HttpRequestMessage req = new(method, uri);

            if (method == HttpMethod.Post)
            {
                if (!debug)
                {
                    var bytes = Zlib.Compress(Encoding.UTF8.GetBytes(data), ZlibCompression.Normal);
                    var byteContent = new ByteArrayContent(bytes);
                    byteContent.Headers.ContentEncoding.Add("deflate");
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    req.Content = byteContent;
                }
                else
                {
                    var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(data));
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    req.Content = byteContent;
                }
            }

            if (debug)
            {
                req.Headers.Add("debug", "1");
            }

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            var response = await _httpClient.SendAsync(req, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();

                if (Zlib.IsCompressed(bytes))
                {
                    bytes = Zlib.Decompress(bytes);
                }

                return bytes;
            }
            else
            {
                throw new Exception($"Failed HTTP request {method} {uri} -> {response.StatusCode}");
            }
        }

        public async Task<byte[]?> GetBundleData(string url, int timeout = 60000)
        {
            return await AsyncRequestFromPath(url, HttpMethod.Get, data: null, timeout);
        }

        public async Task<string> GetJsonAsync(string url)
        {
            var bytes = await AsyncRequestFromPath(url, HttpMethod.Get);
            return Encoding.UTF8.GetString(bytes);
        }

        public string GetJsonBLOCKING(string url)
        {
            return Task.Run(() => GetJsonAsync(url)).GetAwaiter().GetResult();
        }

        public async Task<string> PostJsonAsync(string url, string data, int timeout = DEFAULT_TIMEOUT_MS, int retryAttempts = 5, bool debug = false)
        {
            int attempt = 0;

            while (attempt++ < retryAttempts)
            {
                try
                {
                    // people forget the /
                    if (!url.StartsWith("/"))
                    {
                        url = "/" + url;
                    }

                    var bytes = await AsyncRequestFromPath(url, HttpMethod.Post, data, timeout, debug);
                    return Encoding.UTF8.GetString(bytes);
                }
                catch (Exception ex)
                {
                    StayInTarkovHelperConstants.Logger.LogError($"could not perform request to {url}:\n{ex}");
                }
                await Task.Delay(1000);
            }
            throw new Exception($"Unable to communicate with Aki Server {url} to post json data: {data}");
        }

        public async Task<T> PostJsonAsync<T>(string url, string data, int timeout = DEFAULT_TIMEOUT_MS, int retryAttempts = 5, bool debug = false)
        {
            var rsp = await PostJsonAsync(url, data, timeout, retryAttempts, debug);
            var obj = JsonConvert.DeserializeObject<T>(rsp);
            return obj != null ? obj : throw new Exception($"unexpected null json object after parsing response from {url}: {rsp}");
        }

        internal string PostJsonBLOCKING(string url, string data, int timeout = DEFAULT_TIMEOUT_MS)
        {
            return Task.Run(() => PostJsonAsync(url, data, timeout)).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            SITGameServerClientDataProcessing.OnLatencyUpdated -= OnLatencyUpdated;
        }
    }
}
