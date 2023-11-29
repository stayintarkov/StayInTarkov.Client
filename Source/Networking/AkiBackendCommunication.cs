using Aki.Custom.Airdrops;
using Aki.Custom.Airdrops.Models;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class AkiBackendCommunication : IDisposable
    {
        public const int DEFAULT_TIMEOUT_MS = 5000;
        public const int DEFAULT_TIMEOUT_LONG_MS = 9999;

        private string m_Session;

        public string Session
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
                if (m_Instance == null || m_Instance.Session == null || m_Instance.RemoteEndPoint == null)
                    m_Instance = new AkiBackendCommunication();

                return m_Instance;
            }
        }

        public HttpClient HttpClient { get; set; }

        protected ManualLogSource Logger;

        WebSocketSharp.WebSocket WebSocket { get; set; }

        public static int PING_LIMIT_HIGH { get; } = 125;
        public static int PING_LIMIT_MID { get; } = 100;


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

            GetHeaders();
            ConnectToAkiBackend();
            PeriodicallySendPing();
            PeriodicallySendPooledData();

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

        public async void PostDownWebSocketImmediately(Dictionary<string, object> packet)
        {
            await Task.Run(() =>
            {
                if (WebSocket != null)
                    WebSocket.Send(packet.SITToJson());
            });
        }

        public async void PostDownWebSocketImmediately(string packet)
        {
            await Task.Run(() =>
            {
                if (WebSocket != null)
                    WebSocket.Send(packet);
            });
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
            if (
                CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent)
                && coopGameComponent.LocalGameInstance != null
                )
            {
                coopGameComponent.LocalGameInstance.Stop(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, ExitStatus.Survived, null);
            }
            else
                Application.Quit();
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            GC.AddMemoryPressure(e.RawData.Length);
            try
            {
                //Logger.LogInfo($"Step.0. WebSocket_OnMessage");

                if (sender == null)
                    return;

                if (e == null)
                    return;

                if (string.IsNullOrEmpty(e.Data))
                    return;

                if (e.RawData == null)
                    return;

                if (e.RawData.Length == 0)
                    return;

                Dictionary<string, object> packet = null;
                if (e.Data.IndexOf("{") > 0)
                    return;

                if (!e.Data.EndsWith("}"))
                    return;


                //if (WebSocketPreviousReceived.Contains(e.Data))
                //    return;

                //WebSocketPreviousReceived.Add(e.Data);

                if (DEBUGPACKETS)
                {
                    Logger.LogInfo(e.Data);
                }

                // Use StreamReader & JsonTextReader to improve memory / cpu usage
                using (var streamReader = new StreamReader(new MemoryStream(e.RawData)))
                {
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        packet = serializer.Deserialize<Dictionary<string, object>>(reader);
                    }
                }

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    return;

                if (coopGameComponent == null)
                    return;

                if (packet == null)
                    return;

                //Logger.LogDebug($"Step.1. Packet exists. {packet.ToJson()}");

                // If this is a pong packet, resolve and create a smooth ping
                if (packet.ContainsKey("pong"))
                {
                    var pongRaw = long.Parse(packet["pong"].ToString());
                    var dtPong = new DateTime(pongRaw);
                    var serverPing = (int)(DateTime.UtcNow - dtPong).TotalMilliseconds;
                    if (coopGameComponent.ServerPingSmooth.Count > 60)
                        coopGameComponent.ServerPingSmooth.TryDequeue(out _);
                    coopGameComponent.ServerPingSmooth.Enqueue(serverPing);
                    coopGameComponent.ServerPing = coopGameComponent.ServerPingSmooth.Count > 0 ? (int)Math.Round(coopGameComponent.ServerPingSmooth.Average()) : 1;
                    return;
                }

                if (packet.ContainsKey("HostPing"))
                {
                    var dtHP = new DateTime(long.Parse(packet["HostPing"].ToString()));
                    var timeSpanOfHostToMe = DateTime.UtcNow - dtHP;
                    HostPing = (int)Math.Round(timeSpanOfHostToMe.TotalMilliseconds);
                    return;
                }

                // Syncronize RaidTimer
                if (packet.ContainsKey("RaidTimer"))
                {
                    if (MatchmakerAcceptPatches.IsClient)
                    {
                        var raidTimer = new TimeSpan(long.Parse(packet["RaidTimer"].ToString()));
                        Logger.LogInfo($"RaidTimer: Remaining session time {raidTimer.TraderFormat()}");

                        if (coopGameComponent.LocalGameInstance is CoopGame coopGame)
                        {
                            var gameTimer = coopGame.GameTimer;
                            if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
                            {
                                if (gameTimer.PastTime.TotalSeconds < 3)
                                    return;

                                var timeRemain = gameTimer.PastTime + raidTimer;

                                if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
                                    return;

                                Logger.LogInfo($"RaidTimer: New SessionTime {timeRemain.TraderFormat()}");
                                gameTimer.ChangeSessionTime(timeRemain);

                                // FIXME: Giving SetTime() with empty exfil point arrays has a known bug that may cause client game crashes!
                                coopGame.GameUi.TimerPanel.SetTime(gameTimer.StartDateTime.Value, coopGame.Profile_0.Info.Side, gameTimer.SessionSeconds(), new EFT.Interactive.ExfiltrationPoint[] { });
                            }
                        }
                    }

                    return;
                }

                // Time And Weather
                if (packet.ContainsKey("TimeAndWeather"))
                {
                    if (MatchmakerAcceptPatches.IsClient)
                    {
                        Logger.LogDebug(packet.ToJson());

                        var gameDateTime = new DateTime(long.Parse(packet["GameDateTime"].ToString()));
                        if (coopGameComponent.LocalGameInstance is CoopGame coopGame && coopGame.GameDateTime != null)
                            coopGame.GameDateTime.Reset(gameDateTime);

                        var weatherController = EFT.Weather.WeatherController.Instance;
                        if (weatherController != null)
                        {
                            var weatherDebug = weatherController.WeatherDebug;
                            if (weatherDebug != null)
                            {
                                weatherDebug.Enabled = true;

                                weatherDebug.CloudDensity = float.Parse(packet["CloudDensity"].ToString());
                                weatherDebug.Fog = float.Parse(packet["Fog"].ToString());
                                weatherDebug.LightningThunderProbability = float.Parse(packet["LightningThunderProbability"].ToString());
                                weatherDebug.Rain = float.Parse(packet["Rain"].ToString());
                                weatherDebug.Temperature = float.Parse(packet["Temperature"].ToString());
                                weatherDebug.TopWindDirection = new(float.Parse(packet["TopWindDirection.x"].ToString()), float.Parse(packet["TopWindDirection.y"].ToString()));

                                Vector2 windDirection = new(float.Parse(packet["WindDirection.x"].ToString()), float.Parse(packet["WindDirection.y"].ToString()));

                                // working dog sh*t, if you are the programmer, DON'T EVER DO THIS! - dounai2333
                                static bool BothPositive(float f1, float f2) => f1 > 0 && f2 > 0;
                                static bool BothNegative(float f1, float f2) => f1 < 0 && f2 < 0;
                                static bool VectorIsSameQuadrant(Vector2 v1, Vector2 v2, out int flag)
                                {
                                    flag = 0;
                                    if (v1.x != 0 && v1.y != 0 && v2.x != 0 && v2.y != 0)
                                    {
                                        if (BothPositive(v1.x, v2.x) && BothPositive(v1.y, v2.y)
                                        || BothNegative(v1.x, v2.x) && BothNegative(v1.y, v2.y)
                                        || BothPositive(v1.x, v2.x) && BothNegative(v1.y, v2.y)
                                        || BothNegative(v1.x, v2.x) && BothPositive(v1.y, v2.y))
                                        {
                                            flag = 1;
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        if (v1.x != 0 && v2.x != 0)
                                        {
                                            if (BothPositive(v1.x, v2.x) || BothNegative(v1.x, v2.x))
                                            {
                                                flag = 1;
                                                return true;
                                            }
                                        }
                                        else if (v1.y != 0 && v2.y != 0)
                                        {
                                            if (BothPositive(v1.y, v2.y) || BothNegative(v1.y, v2.y))
                                            {
                                                flag = 2;
                                                return true;
                                            }
                                        }
                                    }
                                    return false;
                                }

                                for (int i = 1; i < WeatherClass.WindDirections.Count(); i++)
                                {
                                    Vector2 direction = WeatherClass.WindDirections[i];
                                    if (VectorIsSameQuadrant(windDirection, direction, out int flag))
                                    {
                                        weatherDebug.WindDirection = (EFT.Weather.WeatherDebug.Direction)i;
                                        weatherDebug.WindMagnitude = flag switch
                                        {
                                            1 => windDirection.x / direction.x,
                                            2 => windDirection.y / direction.y,
                                            _ => weatherDebug.WindMagnitude
                                        };
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogError("TimeAndWeather: WeatherDebug is null!");
                            }
                        }
                        else
                        {
                            Logger.LogError("TimeAndWeather: WeatherController is null!");
                        }
                    }

                    return;
                }

                // If this is an endSession packet, end the session for the clients
                if (packet.ContainsKey("endSession") && MatchmakerAcceptPatches.IsClient)
                {
                    Logger.LogDebug("Received EndSession from Server. Ending Game.");
                    if (coopGameComponent.LocalGameInstance == null)
                        return;

                    coopGameComponent.ServerHasStopped = true;
                    return;
                }

                if (Singleton<SITAirdropsManager>.Instantiated 
                    && packet.ContainsKey("m") 
                    && packet["m"].ToString().StartsWith("Airdrop")
                    )
                {
                    if (packet["m"].ToString() == "AirdropPacket")
                    {
                        Logger.LogInfo("--- RAW AIRDROP PACKET ---");
                        Logger.LogInfo(packet.SITToJson());

                        Singleton<SITAirdropsManager>.Instance.AirdropParameters = packet["model"].ToString().SITParseJson<AirdropParametersModel>();
                    }

                    if (packet["m"].ToString() == "AirdropLootPacket")
                    {
                        Logger.LogInfo("--- RAW AIRDROP-LOOT PACKET ---");
                        Logger.LogInfo(packet.SITToJson());

                        Singleton<SITAirdropsManager>.Instance.ReceiveBuildLootContainer
                            (packet["result"].ToString().SITParseJson<AirdropLootResultModel>()
                            , packet["config"].ToString().SITParseJson<AirdropConfigModel>());
                    }
                }

                // If this is a SIT serialization packet
                if (packet.ContainsKey("data") && packet.ContainsKey("m"))
                {
                    var data = packet["data"];
                    if (data == null)
                        return;
                    //Logger.LogInfo(" =============WebSocket_OnMessage========= ");
                    //Logger.LogInfo(" ==================SIT Packet============= ");
                    //Logger.LogInfo(packet.ToJson());
                    //Logger.LogInfo(" ========================================= ");
                    //if (!packet.ContainsKey("accountId"))
                    if (!packet.ContainsKey("profileId"))
                    {
                        packet.Add("profileId", packet["data"].ToString().Split(',')[0]);
                    }
                }

                // -------------------------------------------------------
                // Add to the Coop Game Component Action Packets
                if (coopGameComponent == null || coopGameComponent.ActionPackets == null || coopGameComponent.ActionPacketHandler == null)
                    return;

                if (packet.ContainsKey("m")
                    && packet["m"].ToString() == "Move")
                    coopGameComponent.ActionPacketHandler.ActionPacketsMovement.TryAdd(packet);
                else if (packet.ContainsKey("m")
                    && packet["m"].ToString() == "ApplyDamageInfo")
                {
                    coopGameComponent.ActionPacketHandler.ActionPacketsDamage.TryAdd(packet);
                }
                else
                    coopGameComponent.ActionPacketHandler.ActionPackets.TryAdd(packet);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
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

        public static bool DEBUGPACKETS { get; } = false;

        public bool HighPingMode { get; set; }
        public BlockingCollection<string> PooledJsonToPost { get; } = new();
        public BlockingCollection<byte[]> PooledBytesToPost { get; } = new();
        public BlockingCollection<KeyValuePair<string, Dictionary<string, object>>> PooledDictionariesToPost { get; } = new();
        public BlockingCollection<List<Dictionary<string, object>>> PooledDictionaryCollectionToPost { get; } = new();

        public BlockingCollection<KeyValuePair<string, string>> PooledJsonToPostToUrl { get; } = new();

        public void SendDataToPool(string url, string serializedData)
        {
            PooledJsonToPostToUrl.Add(new(url, serializedData));
        }

        public void SendDataToPool(string serializedData)
        {
            if (WebSocket != null && WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
                WebSocket.Send(serializedData);
        }

        public void SendDataToPool(byte[] serializedData)
        {
            if (HighPingMode)
            {
                PooledBytesToPost.Add(serializedData);
            }
            else
            {
                if (WebSocket != null && WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
                    WebSocket.Send(serializedData);
            }
        }

        public void SendDataToPool(string url, Dictionary<string, object> data)
        {
            PooledDictionariesToPost.Add(new(url, data));
        }

        public void SendListDataToPool(string url, List<Dictionary<string, object>> data)
        {
            PooledDictionaryCollectionToPost.Add(data);
        }

        public int HostPing { get; private set; } = 1;
        public int PostPing { get; private set; } = 1;
        public ConcurrentQueue<int> PostPingSmooth { get; } = new();

        private Task PeriodicallySendPooledDataTask;

        private void PeriodicallySendPooledData()
        {
            //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData()");

            PeriodicallySendPooledDataTask = Task.Run(async () =>
            {
                int awaitPeriod = 1;
                //GCHelpers.EnableGC();
                //GCHelpers.ClearGarbage();
                //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData():In Async Task");

                //while (m_Instance != null)
                Stopwatch swPing = new();

                while (true)
                {
                    if (WebSocket == null)
                    {
                        await Task.Delay(awaitPeriod);
                        continue;
                    }

                    swPing.Restart();
                    await Task.Delay(awaitPeriod);

                    while (PooledBytesToPost.Any())
                    {
                        await Task.Delay(awaitPeriod);
                        if (WebSocket != null)
                        {
                            if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
                            {
                                while (PooledBytesToPost.TryTake(out var bytes))
                                {
                                    WebSocket.Send(bytes);
                                }
                            }
                            else
                            {
                                WebSocket_OnError();
                            }
                        }
                    }
                    //await Task.Delay(100);
                    while (PooledDictionariesToPost.Any())
                    {
                        await Task.Delay(awaitPeriod);

                        KeyValuePair<string, Dictionary<string, object>> d;
                        if (PooledDictionariesToPost.TryTake(out d))
                        {

                            var url = d.Key;
                            var json = JsonConvert.SerializeObject(d.Value);
                            //var json = d.Value.ToJson();
                            if (WebSocket != null)
                            {
                                if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
                                {
                                    WebSocket.Send(json);
                                }
                                else
                                {
                                    WebSocket_OnError();
                                }
                            }
                        }
                    }

                    if (PooledDictionaryCollectionToPost.TryTake(out var d2))
                    {
                        var json = JsonConvert.SerializeObject(d2);
                        if (WebSocket != null)
                        {
                            if (WebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
                            {
                                WebSocket.Send(json);
                            }
                            else
                            {
                                StayInTarkovHelperConstants.Logger.LogError($"WS:Periodic Send:PooledDictionaryCollectionToPost:Failed!");
                            }
                        }
                        json = null;
                    }

                    while (PooledJsonToPostToUrl.Any())
                    {
                        await Task.Delay(awaitPeriod);

                        if (PooledJsonToPostToUrl.TryTake(out var kvp))
                        {
                            _ = await PostJsonAsync(kvp.Key, kvp.Value, timeout: 1000, debug: true);
                        }
                    }

                    if (PostPingSmooth.Any() && PostPingSmooth.Count > 30)
                        PostPingSmooth.TryDequeue(out _);

                    PostPingSmooth.Enqueue((int)swPing.ElapsedMilliseconds - awaitPeriod);
                    PostPing = (int)Math.Round(PostPingSmooth.Average());
                }
            });
        }

        private Task PeriodicallySendPingTask { get; set; }

        private void PeriodicallySendPing()
        {
            PeriodicallySendPingTask = Task.Run(async () =>
            {
                int awaitPeriod = 2000;
                while (true)
                {
                    await Task.Delay(awaitPeriod);

                    if (WebSocket == null)
                        continue;

                    if (WebSocket.ReadyState != WebSocketSharp.WebSocketState.Open)
                        continue;

                    if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                        continue;

                    // PatchConstants.Logger.LogDebug($"WS:Ping Send");

                    var packet = new
                    {
                        m = "Ping",
                        t = DateTime.UtcNow.Ticks.ToString("G"),
                        profileId = coopGameComponent.OwnPlayer.ProfileId,
                        serverId = coopGameComponent.ServerId
                    };

                    WebSocket.Send(Encoding.UTF8.GetBytes(packet.ToJson()));
                    packet = null;
                }
            });
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
                    Session = arg.Replace("-token=", string.Empty);
                    m_RequestHeaders = new Dictionary<string, string>()
                        {
                            { "Cookie", $"PHPSESSID={Session}" },
                            { "SessionId", Session }
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

        /// <summary>
        /// TODO: Replace this with a HTTPClient Post command. 
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ServerCertificateValidationCallback = delegate { return true; };

            foreach (var item in GetHeaders())
            {
                request.Headers.Add(item.Key, item.Value);
            }

            if (!debug && method == "POST")
            {
                request.Headers.Add("Accept-Encoding", "deflate");
            }

            request.Method = method;
            request.Timeout = timeout;

            if (!string.IsNullOrEmpty(data))
            {
                if (debug && method == "POST")
                {
                    compress = false;
                    request.Headers.Add("debug", "1");
                }

                // set request body
                var inputDataBytes = Encoding.UTF8.GetBytes(data);
                //byte[] bytes = compress ? Zlib.Compress(inputDataBytes, ZlibCompression.Fastest) : inputDataBytes;
                byte[] bytes = compress ? Zlib.Compress(data) : inputDataBytes;
                data = null;
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;
                if (compress)
                    request.Headers.Add("content-encoding", "deflate");

                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception e)
                {
                    StayInTarkovHelperConstants.Logger.LogError(e);
                }
                finally
                {
                    bytes = null;
                    inputDataBytes = null;
                }
            }

            // get response stream
            var ms = new MemoryStream();
            try
            {
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                        responseStream.CopyTo(ms);
                }
            }
            catch (Exception e)
            {
                StayInTarkovHelperConstants.Logger.LogError(e);
            }
            finally
            {
                request = null;
                uri = null;
            }
            return ms;
        }

        public byte[] GetData(string url, bool hasHost = false)
        {
            using (var dataStream = SendAndReceive(url, "GET"))
                return dataStream.ToArray();
        }

        public void PutJson(string url, string data, bool compress = true, int timeout = 9999, bool debug = false)
        {
            using (Stream stream = SendAndReceive(url, "PUT", data, compress, timeout, debug)) { }
        }

        public string GetJson(string url, bool compress = true, int timeout = 9999)
        {
            using (MemoryStream stream = SendAndReceive(url, "GET", null, compress, timeout))
            {
                if (stream == null)
                    return "";
                var bytes = stream.ToArray();
                var result = Zlib.Decompress(bytes);
                bytes = null;
                return result;
            }
        }

        public string PostJson(string url, string data, bool compress = true, int timeout = 9999, bool debug = false)
        {
            using (MemoryStream stream = SendAndReceive(url, "POST", data, compress, timeout, debug))
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
        }

        public async Task<string> PostJsonAsync(string url, string data, bool compress = true, int timeout = DEFAULT_TIMEOUT_MS, bool debug = false, int retryAttempts = 5)
        {
            int attempt = 0;
            while (attempt++ < retryAttempts)
            {
                try
                {
                    return await Task.FromResult(PostJson(url, data, compress, timeout, debug));
                }
                catch (Exception ex)
                {
                    StayInTarkovHelperConstants.Logger.LogError(ex);
                }
            }
            throw new Exception($"Unable to communicate with Aki Server {url} to post json data: {data}");
        }

        public void PostJsonAndForgetAsync(string url, string data, bool compress = true, int timeout = DEFAULT_TIMEOUT_LONG_MS, bool debug = false)
        {
            SendDataToPool(url, data);
            //try
            //{
            //    _ = Task.Run(() => PostJson(url, data, compress, timeout, debug));
            //}
            //catch (Exception ex)
            //{
            //    PatchConstants.Logger.LogError(ex);
            //}
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
            Session = null;
            RemoteEndPoint = null;
        }
    }
}
