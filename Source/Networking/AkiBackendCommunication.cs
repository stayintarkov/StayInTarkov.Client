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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class AkiBackendCommunication : IDisposable
    {
        public const int DEFAULT_TIMEOUT_MS = 9999;
        public const int DEFAULT_TIMEOUT_LONG_MS = 9999;
        public const string PACKET_TAG_METHOD = "m";
        public const string PACKET_TAG_SERVERID = "serverId";
        public const string PACKET_TAG_DATA = "data";

        private string m_Session;

        public string ProfileId
        {
            get
            {
                return m_Session;
            }
            set { m_Session = value; }
        }



        private string m_RemoteEndPoint;

        public string RemoteEndPoint
        {
            get
            {
                if (string.IsNullOrEmpty(m_RemoteEndPoint))
                    m_RemoteEndPoint = StayInTarkovHelperConstants.GetBackendUrl();

                // Remove ending slash on URI for SIT.Manager.Avalonia
                if(m_RemoteEndPoint.EndsWith("/"))
                    m_RemoteEndPoint = m_RemoteEndPoint.Substring(0, m_RemoteEndPoint.Length - 1);

                return m_RemoteEndPoint;

            }
            set { m_RemoteEndPoint = value; }
        }

        private Dictionary<string, string>? m_RequestHeaders = null;

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

        public HttpClient HttpClient { get; set; }

        protected ManualLogSource Logger;

        public WebSocketSharp.WebSocket WebSocket { get; private set; }

        public long BytesSent = 0;
        public long BytesReceived = 0;
        public ushort Ping = 0;
        public ConcurrentQueue<int> ServerPingSmooth { get; } = new();

        public static int PING_LIMIT_HIGH { get; } = 125;
        public static int PING_LIMIT_MID { get; } = 100;
        public static bool IsLocal;


        protected AkiBackendCommunication(ManualLogSource logger = null)
        {
            // disable SSL encryption
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            if (logger != null)
                Logger = logger;
            else
                Logger = BepInEx.Logging.Logger.CreateLogSource("Request");

            if (string.IsNullOrEmpty(RemoteEndPoint))
                RemoteEndPoint = StayInTarkovHelperConstants.GetBackendUrl();

            IsLocal = RemoteEndPoint.Contains("127.0.0.1")
                    || RemoteEndPoint.Contains("localhost");

            GetHeaders();
            ConnectToAkiBackend();
            PeriodicallySendPing();
            //PeriodicallySendPooledData();

            var processor = StayInTarkovPlugin.Instance.GetOrAddComponent<SITGameServerClientDataProcessing>();
            Singleton<SITGameServerClientDataProcessing>.Create(processor);
            Comfort.Common.Singleton<SITGameServerClientDataProcessing>.Instance.OnLatencyUpdated += OnLatencyUpdated;

            HttpClient = new HttpClient();
            foreach (var item in GetHeaders())
            {
                HttpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
            HttpClient.MaxResponseContentBufferSize = long.MaxValue;
            HttpClient.Timeout = new TimeSpan(0, 0, 0, 0, 1000);

            HighPingMode = PluginConfigSettings.Instance.CoopSettings.ForceHighPingMode;
        }

        private void ConnectToAkiBackend()
        {
            PooledJsonToPostToUrl.Add(new KeyValuePair<string, string>("/coop/connect", "{}"));
        }

        private Profile MyProfile { get; set; }

        //private HashSet<string> WebSocketPreviousReceived { get; set; }

        public void WebSocketCreate(Profile profile)
        {
            MyProfile = profile;

            Logger.LogDebug("WebSocketCreate");
            //if (WebSocket != null && WebSocket.ReadyState != WebSocketSharp.WebSocketState.Closed)
            //{
            //    Logger.LogDebug("WebSocketCreate:WebSocket already exits");
            //    return;
            //}

            Logger.LogDebug("Request Instance is connecting to WebSocket");

            var webSocketPort = PluginConfigSettings.Instance.CoopSettings.SITWebSocketPort;
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{webSocketPort}/{profile.ProfileId}?";
            Logger.LogDebug(webSocketPort);
            Logger.LogDebug(StayInTarkovHelperConstants.GetREALWSURL());
            Logger.LogDebug(wsUrl);

            //WebSocketPreviousReceived = new HashSet<string>();
            WebSocket = new WebSocketSharp.WebSocket(wsUrl);
            WebSocket.WaitTime = TimeSpan.FromMinutes(1);
            WebSocket.EmitOnPing = true;
            WebSocket.Connect();

            // ---
            // Start up after initial Send
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
            // ---
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
            Logger.LogError($"{nameof(WebSocket_OnError)}: {e.Message} {Environment.NewLine}");
            Logger.LogError($"{nameof(WebSocket_OnError)}: {e.Exception}");
            WebSocket_OnError();
            WebSocketClose();
            WebSocketCreate(MyProfile);
        }

        private void WebSocket_OnError()
        {
            Logger.LogError($"Your PC has failed to connect and send data to the WebSocket with the port {PluginConfigSettings.Instance.CoopSettings.SITWebSocketPort} on the Server {StayInTarkovHelperConstants.GetBackendUrl()}! Application will now close.");
            if (Singleton<ISITGame>.Instantiated)
            {
                Singleton<ISITGame>.Instance.Stop(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, Singleton<ISITGame>.Instance.MyExitStatus, Singleton<ISITGame>.Instance.MyExitLocation);
            }
            else
                Application.Quit();
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (e == null)
                return;

            Interlocked.Add(ref BytesReceived, e.RawData.Length);
            GC.AddMemoryPressure(e.RawData.Length);

            var d = e.RawData;
            if (d.Length >= 3 && d[0] != '{' && !(d[0] == 'S' && d[1] == 'I' && d[2] == 'T'))
            {
                Singleton<SITGameServerClientDataProcessing>.Instance.ProcessFlatBuffer(d);
            }
            else
            {
                Singleton<SITGameServerClientDataProcessing>.Instance.ProcessPacketBytes(e.RawData);
            }
            GC.RemoveMemoryPressure(e.RawData.Length);
        }

        public static AkiBackendCommunication GetRequestInstance(bool createInstance = false, ManualLogSource logger = null)
        {
            if (createInstance)
            {
                return new AkiBackendCommunication(logger);
            }

            return Instance;
        }

        public bool HighPingMode { get; set; }

        public BlockingCollection<byte[]> PooledBytesToPost { get; } = new BlockingCollection<byte[]>();
        public BlockingCollection<KeyValuePair<string, Dictionary<string, object>>> PooledDictionariesToPost { get; } = new();
        public BlockingCollection<List<Dictionary<string, object>>> PooledDictionaryCollectionToPost { get; } = new();

        public BlockingCollection<KeyValuePair<string, string>> PooledJsonToPostToUrl { get; } = new();

        //public void SendDataToPool(string url, string serializedData)
        //{
        //    PooledJsonToPostToUrl.Add(new(url, serializedData));
        //}

        //public void SendDataToPool(string serializedData)
        //{
        //    if (WebSocket != null && WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        //        WebSocket.Send(serializedData);
        //}

        private HashSet<string> _previousPooledData = new HashSet<string>();

        //public void SendDataToPool(byte[] serializedData)
        //{

        //    if (DEBUGPACKETS)
        //    {
        //        Logger.LogDebug(nameof(SendDataToPool));
        //        Logger.LogDebug(Encoding.UTF8.GetString(serializedData));
        //    }
        //    //if (_previousPooledData.Contains(Encoding.UTF8.GetString(serializedData)))
        //    //    return;

        //    //_previousPooledData.Add(Encoding.UTF8.GetString(serializedData));   
        //    PooledBytesToPost.Add(serializedData);

        //    //if (HighPingMode)
        //    //{
        //    //    PooledBytesToPost.Add(serializedData);
        //    //}
        //    //else
        //    //{
        //    //    if (WebSocket != null && WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        //    //        WebSocket.Send(serializedData);
        //    //}
        //}

        //public void SendDataToPool(string url, Dictionary<string, object> data)
        //{
        //    PooledDictionariesToPost.Add(new(url, data));
        //}

        //public void SendListDataToPool(string url, List<Dictionary<string, object>> data)
        //{
        //    PooledDictionaryCollectionToPost.Add(data);
        //}

        //private Task PeriodicallySendPooledDataTask;

        //private void PeriodicallySendPooledData()
        //{
        //    //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData()");

        //    PeriodicallySendPooledDataTask = Task.Run(async () =>
        //    {
        //        int awaitPeriod = 1;
        //        //GCHelpers.EnableGC();
        //        //GCHelpers.ClearGarbage();
        //        //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData():In Async Task");

        //        //while (m_Instance != null)
        //        Stopwatch swPing = new();

        //        while (true)
        //        {
        //            if (WebSocket == null)
        //            {
        //                await Task.Delay(awaitPeriod);
        //                continue;
        //            }

        //            // If there is nothing to post. Then delay 1ms (to avoid mem leak) and continue.
        //            if
        //            (
        //                !PooledBytesToPost.Any()
        //                && !PooledDictionariesToPost.Any()
        //                && !PooledDictionaryCollectionToPost.Any()
        //                && !PooledJsonToPostToUrl.Any()
        //            )
        //            {
        //                swPing.Restart();
        //                await Task.Delay(awaitPeriod);
        //                continue;
        //            }

        //            // This would the most common delivery from the Client
        //            // Pooled up bytes will now send to the Web Socket
        //            while (PooledBytesToPost.Any())
        //            {
        //                //await Task.Delay(awaitPeriod);
        //                if (WebSocket != null)
        //                {
        //                    if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        //                    {
        //                        while (PooledBytesToPost.TryTake(out var bytes))
        //                        {
        //                            //Logger.LogDebug($"Sending bytes of {bytes.Length}b in length");
        //                            if (DEBUGPACKETS)
        //                            {
        //                                Logger.LogDebug($"SENT:{Encoding.UTF8.GetString(bytes)}");
        //                            }

        //                            WebSocket.Send(bytes);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        WebSocket_OnError();
        //                    }
        //                }
        //            }
        //            //await Task.Delay(100);
        //            while (PooledDictionariesToPost.Any())
        //            {
        //                await Task.Delay(awaitPeriod);

        //                KeyValuePair<string, Dictionary<string, object>> d;
        //                if (PooledDictionariesToPost.TryTake(out d))
        //                {

        //                    var url = d.Key;
        //                    var json = JsonConvert.SerializeObject(d.Value);
        //                    //var json = d.Value.ToJson();
        //                    if (WebSocket != null)
        //                    {
        //                        if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        //                        {
        //                            WebSocket.Send(json);
        //                        }
        //                        else
        //                        {
        //                            WebSocket_OnError();
        //                        }
        //                    }
        //                }
        //            }

        //            if (PooledDictionaryCollectionToPost.TryTake(out var d2))
        //            {
        //                var json = JsonConvert.SerializeObject(d2);
        //                if (WebSocket != null)
        //                {
        //                    if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        //                    {
        //                        WebSocket.Send(json);
        //                    }
        //                    else
        //                    {
        //                        StayInTarkovHelperConstants.Logger.LogError($"WS:Periodic Send:PooledDictionaryCollectionToPost:Failed!");
        //                    }
        //                }
        //                json = null;
        //            }

        //            while (PooledJsonToPostToUrl.Any())
        //            {
        //                await Task.Delay(awaitPeriod);

        //                if (PooledJsonToPostToUrl.TryTake(out var kvp))
        //                {
        //                    _ = await PostJsonAsync(kvp.Key, kvp.Value, timeout: 1000, debug: true);
        //                }
        //            }

        //            if (PostPingSmooth.Any() && PostPingSmooth.Count > 30)
        //                PostPingSmooth.TryDequeue(out _);

        //            PostPingSmooth.Enqueue((int)swPing.ElapsedMilliseconds - awaitPeriod);
        //            PostPing = (int)Math.Round(PostPingSmooth.Average());
        //        }
        //    });
        //}

        private Task PeriodicallySendPingTask { get; set; }

        private void PeriodicallySendPing()
        {
            PeriodicallySendPingTask = Task.Run(async () =>
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

        private Dictionary<string, string> GetHeaders()
        {
            if (m_RequestHeaders != null && m_RequestHeaders.Count > 0)
                return m_RequestHeaders;

            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                if (arg.Contains("-token="))
                {
                    ProfileId = arg.Replace("-token=", string.Empty);
                    m_RequestHeaders = new Dictionary<string, string>()
                        {
                            { "Cookie", $"PHPSESSID={ProfileId}" },
                            { "SessionId", ProfileId }
                        };
                    break;
                }
            }
            return m_RequestHeaders;
        }

        /// <summary>
        /// Send request to the server and get Stream of data back
        /// </summary>
        /// <param name="path">String url endpoint example: /start</param>
        /// <param name="method">POST or GET</param>
        /// <param name="data">string json data</param>
        /// <param name="compress">Should use compression gzip?</param>
        /// <returns>Stream or null</returns>
        private async Task<byte[]?> asyncRequestFromPath(string path, string method = "GET", string? data = null, int timeout = 9999, bool debug = false)
        {
            if (!Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                path = RemoteEndPoint + path;
            }

            return await asyncRequest(new Uri(path), method, data, timeout, debug);
        }

        private async Task<byte[]?> asyncRequest(Uri uri, string method = "GET", string? data = null, int timeout = 9999, bool debug = false)
        {
            var compress = true;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    handler.UseCookies = true;
                    handler.CookieContainer = new CookieContainer();
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    Uri baseAddress = new Uri(RemoteEndPoint);
                    foreach (var item in GetHeaders())
                    {
                        if (item.Key == "Cookie")
                        {
                            string[] pairs = item.Value.Split(';');
                            var keyValuePairs = pairs
                                .Select(p => p.Split(new[] { '=' }, 2))
                                .Where(kvp => kvp.Length == 2)
                                .ToDictionary(kvp => kvp[0], kvp => kvp[1]);
                            foreach (var kvp in keyValuePairs)
                            {
                                handler.CookieContainer.Add(baseAddress, new Cookie(kvp.Key, kvp.Value));
                            }
                        }
                        else
                        {
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    if (!debug && method == "POST")
                    {
                        httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");
                    }

                    HttpContent? byteContent = null;
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
                    {
                        if (debug)
                        {
                            compress = false;
                            httpClient.DefaultRequestHeaders.Add("debug", "1");
                        }
                        var inputDataBytes = Encoding.UTF8.GetBytes(data);
                        var bytes = compress ? Zlib.Compress(inputDataBytes, ZlibCompression.Normal) : inputDataBytes;
                        byteContent = new ByteArrayContent(bytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        if (compress)
                        {
                            byteContent.Headers.ContentEncoding.Add("deflate");
                        }
                    }

                    HttpResponseMessage response;
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        response = await httpClient.PostAsync(uri, byteContent);
                    }
                    else
                    {
                        response = await httpClient.GetAsync(uri);
                    }

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
                        StayInTarkovHelperConstants.Logger.LogError($"Unable to send api request to server.Status code" + response.StatusCode);
                        return null;
                    }
                }

            }
        }

        public async Task<byte[]?> GetBundleData(string url, int timeout = 60000)
        {
            return await asyncRequestFromPath(url, "GET", data: null, timeout);
        }

        public async Task<string> GetJsonAsync(string url, int maxRetries = 3, int delayMs = 1000)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    var bytes = await asyncRequestFromPath(url, "GET");
                    return Encoding.UTF8.GetString(bytes);
                }
                catch (Exception) when (attempt < maxRetries)
                {
                    Logger.LogWarning("[ CONFAIL ] Connection failed, retrying! (#"+ attempt +")");
                    attempt++;
                    await Task.Delay(delayMs);
                }
            }
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

                    var bytes = await asyncRequestFromPath(url, "POST", data, timeout, debug);
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

        /// <summary>
        /// Retrieves data asyncronously and parses response JSON to the desired type
        /// </summary>
        /// <typeparam name="T">Desired type to Deserialize to</typeparam>
        /// <param name="url">URL to call</param>
        /// <param name="data">data to send</param>
        /// <returns></returns>
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
            ProfileId = null;
            RemoteEndPoint = null;
            Singleton<SITGameServerClientDataProcessing>.Instance.OnLatencyUpdated -= OnLatencyUpdated;
        }
    }
}
