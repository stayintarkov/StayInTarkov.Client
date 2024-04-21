using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

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

        //public bool isUnity;
        private Dictionary<string, string> m_RequestHeaders { get; set; }

        private static AkiBackendCommunication m_Instance { get; set; }
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
            WebSocket.Send("CONNECTED FROM SIT COOP");
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

            Comfort.Common.Singleton<SITGameServerClientDataProcessing>.Instance.ProcessPacketBytes(e.RawData);
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
        /// <param name="url">String url endpoint example: /start</param>
        /// <param name="method">POST or GET</param>
        /// <param name="data">string json data</param>
        /// <param name="compress">Should use compression gzip?</param>
        /// <returns>Stream or null</returns>
        private MemoryStream SendAndReceive(string url, string method = "GET", string data = null, bool compress = true, int timeout = 9999, bool debug = false)
        {
            // Force to DEBUG mode if not Compressing.
            debug = debug || !compress;

            HttpClient.Timeout = TimeSpan.FromMilliseconds(timeout);


            method = method.ToUpper();

            var fullUri = url;
            if (!Uri.IsWellFormedUriString(fullUri, UriKind.Absolute))
                fullUri = RemoteEndPoint + fullUri;

            if (method == "GET")
            {
                var ms = new MemoryStream();
                var stream = HttpClient.GetStreamAsync(fullUri);
                stream.Result.CopyTo(ms);
                return ms;
            }
            else if (method == "POST" || method == "PUT")
            {
                var uri = new Uri(fullUri);
                return SendAndReceivePostOld(uri, method, data, compress, timeout, debug);
            }

            throw new ArgumentException($"Unknown method {method}");
        }

        // <summary>
        /// Send request to the server and get Stream of data back
        /// </summary>
        /// <param name="url">String url endpoint example: /start</param>
        /// <param name="method">POST or GET</param>
        /// <param name="data">string json data</param>
        /// <param name="compress">Should use compression gzip?</param>
        /// <returns>Stream or null</returns>
        private async Task<MemoryStream> SendAndReceiveAsync(string url, string method = "GET", string data = null, bool compress = true, int timeout = 9999, bool debug = false)
        {
            // Force to DEBUG mode if not Compressing.
            debug = debug || !compress;

            HttpClient.Timeout = TimeSpan.FromMilliseconds(timeout);


            method = method.ToUpper();

            var fullUri = url;
            if (!Uri.IsWellFormedUriString(fullUri, UriKind.Absolute))
                fullUri = RemoteEndPoint + fullUri;

            if (method == "GET")
            {
                var ms = new MemoryStream();
                var stream = await HttpClient.GetStreamAsync(fullUri);
                stream.CopyTo(ms);
                return ms;
            }
            else if (method == "POST" || method == "PUT")
            {
                var uri = new Uri(fullUri);
                return await SendAndReceivePostAsync(uri, method, data, compress, timeout, debug);
            }

            throw new ArgumentException($"Unknown method {method}");
        }

        /// <summary>
        /// Send request to the server and get Stream of data back by post
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="compress"></param>
        /// <param name="timeout"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        MemoryStream SendAndReceivePostOld(Uri uri, string method = "GET", string data = null, bool compress = true, int timeout = 9999, bool debug = false)
        {
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

                    HttpContent byteContent = null;
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
                    {
                        if (debug)
                        {
                            compress = false;
                            httpClient.DefaultRequestHeaders.Add("debug", "1");
                        }
                        var inputDataBytes = Encoding.UTF8.GetBytes(data);
                        byte[] bytes = compress ? Zlib.Compress(inputDataBytes) : inputDataBytes;
                        byteContent = new ByteArrayContent(bytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        if (compress)
                        {
                            byteContent.Headers.ContentEncoding.Add("deflate");
                        }
                    }

                    HttpResponseMessage response;
                    if (byteContent != null)
                    {
                        response = httpClient.PostAsync(uri, byteContent).Result;
                    }
                    else
                    {
                        response = method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                            ? httpClient.PostAsync(uri, null).Result
                            : httpClient.GetAsync(uri).Result;
                    }

                    var ms = new MemoryStream();
                    if (response.IsSuccessStatusCode)
                    {
                        Stream responseStream = response.Content.ReadAsStreamAsync().Result;
                        responseStream.CopyTo(ms);
                        responseStream.Dispose();
                    }
                    else
                    {
                        StayInTarkovHelperConstants.Logger.LogError($"Unable to send api request to server.Status code" + response.StatusCode);
                    }

                    return ms;
                }

            }
        }

        async Task<MemoryStream> SendAndReceivePostAsync(Uri uri, string method = "GET", string data = null, bool compress = true, int timeout = 9999, bool debug = false)
        {
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

                    HttpContent byteContent = null;
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(data))
                    {
                        if (debug)
                        {
                            compress = false;
                            httpClient.DefaultRequestHeaders.Add("debug", "1");
                        }
                        var inputDataBytes = Encoding.UTF8.GetBytes(data);
                        byte[] bytes = compress ? Zlib.Compress(inputDataBytes) : inputDataBytes;
                        byteContent = new ByteArrayContent(bytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        if (compress)
                        {
                            byteContent.Headers.ContentEncoding.Add("deflate");
                        }
                    }

                    HttpResponseMessage response;
                    if (byteContent != null)
                    {
                        response = await httpClient.PostAsync(uri, byteContent);
                    }
                    else
                    {
                        response = method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                            ? await httpClient.PostAsync(uri, null)
                            : await httpClient.GetAsync(uri);
                    }

                    var ms = new MemoryStream();
                    if (response.IsSuccessStatusCode)
                    {
                        Stream responseStream = await response.Content.ReadAsStreamAsync();
                        responseStream.CopyTo(ms);
                        responseStream.Dispose();
                    }
                    else
                    {
                        StayInTarkovHelperConstants.Logger.LogError($"Unable to send api request to server.Status code" + response.StatusCode);
                    }

                    return ms;
                }

            }
        }

        public byte[] GetData(string url, bool hasHost = false)
        {
            using (var dataStream = SendAndReceive(url, "GET"))
                return dataStream.ToArray();
        }

        public byte[] GetBundleData(string url, int timeout = 300000)
        {
            using (var dataStream = SendAndReceive(url, "GET", null, true, timeout))
                return dataStream.ToArray();
        }

        public void PutJson(string url, string data, bool compress = true, int timeout = 9999, bool debug = false)
        {
            using (Stream stream = SendAndReceive(url, "PUT", data, compress, timeout, debug)) { }
        }

        //public string GetJson(string url, bool compress = true, int timeout = 9999)
        //{
        //    using (MemoryStream stream = SendAndReceive(url, "GET", null, compress, timeout))
        //    {
        //        if (stream == null)
        //            return "";
        //        var bytes = stream.ToArray();
        //        var result = Zlib.Decompress(bytes);
        //        bytes = null;
        //        return result;
        //    }
        //}

        public string GetJson(string url, bool compress = true, int timeout = 9999)
        {
            string result = null;
            int attempts = 10;
            while (result == null && attempts-- > 0)
            {
                using (MemoryStream stream = SendAndReceive(url, "GET", null, compress, timeout))
                {
                    if (stream == null)
                        return "";
                    var bytes = stream.ToArray();
                    result = Zlib.Decompress(bytes);
                    bytes = null;
                }
            }
            return result;
        }

        public string PostJson(string url, string data, bool compress = true, int timeout = 9999, bool debug = false)
        {
            // people forget the /
            if(!url.StartsWith("/"))
                url = "/" + url;

            using (MemoryStream stream = SendAndReceive(url, "POST", data, compress, timeout, debug))
            {
                return ConvertStreamToString(compress, stream);
            }
        }

        private static string ConvertStreamToString(bool compress, MemoryStream stream)
        {
            if (stream == null)
                return "";

            var bytes = stream.ToArray();
            string resultString;

            if (compress)
            {
                if (Zlib.IsCompressed(bytes))
                    resultString = Zlib.Decompress(bytes);
                else
                    resultString = Encoding.UTF8.GetString(bytes);
            }
            else
            {
                resultString = Encoding.UTF8.GetString(bytes);
            }

            return resultString;
        }

        public async Task<string> PostJsonAsync(string url, string data, bool compress = true, int timeout = DEFAULT_TIMEOUT_MS, bool debug = false, int retryAttempts = 5)
        {
            int attempt = 0;
            Task<MemoryStream> sendReceiveTask;

            while (attempt++ < retryAttempts)
            {
                try
                {
                    // people forget the /
                    if (!url.StartsWith("/"))
                        url = "/" + url;

                    sendReceiveTask = SendAndReceiveAsync(url, "POST", data, compress, timeout, debug);
                    using (MemoryStream stream = await sendReceiveTask)
                    {
                        sendReceiveTask.Dispose();
                        sendReceiveTask = null;
                        return ConvertStreamToString(compress, stream);
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    StayInTarkovHelperConstants.Logger.LogError(new System.Diagnostics.StackTrace());
                    StayInTarkovHelperConstants.Logger.LogError(ex);
#endif
                }
                await Task.Delay(1000);
            }
            throw new Exception($"Unable to communicate with Aki Server {url} to post json data: {data}");
        }

        public void PostJsonAndForgetAsync(string url, string data, bool compress = true, int timeout = DEFAULT_TIMEOUT_LONG_MS, bool debug = false)
        {
            Task.Run(() => PostJson(url, data, compress, timeout, debug));
        }


        /// <summary>
        /// Retrieves data asyncronously and parses to the desired type
        /// </summary>
        /// <typeparam name="T">Desired type to Deserialize to</typeparam>
        /// <param name="url">URL to call</param>
        /// <param name="data">data to send</param>
        /// <returns></returns>
        public async Task<T> PostJsonAsync<T>(string url, string data, int timeout = DEFAULT_TIMEOUT_MS, int retryAttempts = 5, bool debug = true)
        {
            int attempt = 0;
            while (attempt++ < retryAttempts)
            {
                try
                {
                    var json = await PostJsonAsync(url, data, compress: false, timeout: timeout, debug);
                    return await Task.Run(() => JsonConvert.DeserializeObject<T>(json));
                }
                catch (Exception ex)
                {
                    StayInTarkovHelperConstants.Logger.LogError(ex);
                }
            }
            throw new Exception($"Unable to communicate with Aki Server {url} to post json data: {data}");
        }

        public void Dispose()
        {
            ProfileId = null;
            RemoteEndPoint = null;
            Comfort.Common.Singleton<SITGameServerClientDataProcessing>.Instance.OnLatencyUpdated -= OnLatencyUpdated;
        }
    }
}
