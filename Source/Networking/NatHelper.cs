using BepInEx.Logging;
using EFT.UI;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Open.Nat;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using STUN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StayInTarkov.Networking
{

    public enum NatTraversalMethod
    {
        Upnp,
        NatPunch,
        PortForward
    }

    public class NatHelper
    {
        public LiteNetLib.NetManager NetManager;
        public WebSocket WebSocket { get; set; }
        public Dictionary<string, TaskCompletionSource<object>> RequestCompletionSourceList = new Dictionary<string, TaskCompletionSource<object>>();
        public Dictionary<string, IPEndPoint> PublicEndPoints = new Dictionary<string, IPEndPoint>();

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Nat Helper");
        private readonly string _sessionId;

        public NatHelper(LiteNetLib.NetManager netManager, string sid)
        {
            NetManager = netManager;
            _sessionId = sid;
        }

        public void Connect()
        {
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{PluginConfigSettings.Instance.CoopSettings.SITNatHelperPort}/{_sessionId}?";

            WebSocket = new WebSocket(wsUrl);
            WebSocket.WaitTime = TimeSpan.FromMinutes(1);
            WebSocket.EmitOnPing = true;
            WebSocket.Connect();

            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
        }

        public void Close()
        {
            WebSocket.Close();
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (e == null)
                return;

            if (string.IsNullOrEmpty(e.Data))
                return;

            ProcessMessage(e.Data);
        }

        private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Logger.LogInfo("WebSocket Error:" + e.Message);
            WebSocket.Close();
        }

        private void ProcessMessage(string message)
        {
            JObject msgObj = JObject.Parse(message);

            if (msgObj.ContainsKey("requestId") && msgObj.ContainsKey("requestType") && msgObj.ContainsKey("profileId"))
            {
                var requestId = msgObj["requestId"].ToString();
                var requestType = msgObj["requestType"].ToString();
                var profileId = msgObj["profileId"].ToString();

                if (requestType == "getEndPointsRequest")
                {
                    var getEndPointsResponse = new Dictionary<string, object>
                    {
                        { "serverId", _sessionId },
                        { "requestId", requestId },
                        { "requestType", "getEndPointsResponse" },
                        { "profileId", profileId },
                        { "publicEndPoints", PublicEndPoints.ToDictionary(x => x.Key, x => x.Value.ToString()) }
                    };

                    WebSocket.Send(JsonConvert.SerializeObject(getEndPointsResponse));
                }

                if (requestType == "natPunchRequest")
                {
                    var publicEndPoints = msgObj["publicEndPoints"]
                        .ToObject<Dictionary<string, string>>()
                        .ToDictionary(x => x.Key, x => x.Value.ToIPEndPoint());

                    if (publicEndPoints.ContainsKey("stun"))
                    {
                        PunchNat(publicEndPoints["stun"]);
                    }

                    var natPunchResponse = new Dictionary<string, object>
                    {
                        { "requestId", requestId },
                        { "requestType", "natPunchResponse" },
                        { "profileId", profileId },
                    };

                    WebSocket.Send(JsonConvert.SerializeObject(natPunchResponse));
                }

                if (requestType == "getEndPointsResponse")
                {
                    var publicEndPoints = msgObj["publicEndPoints"]
                        .ToObject<Dictionary<string, string>>()
                        .ToDictionary(x => x.Key, x => x.Value.ToIPEndPoint());

                    if (RequestCompletionSourceList.ContainsKey(requestId))
                        RequestCompletionSourceList[requestId].SetResult(publicEndPoints);
                }

                if (requestType == "natPunchResponse")
                {
                    if (RequestCompletionSourceList.ContainsKey(requestId))
                        RequestCompletionSourceList[requestId].SetResult(true);
                }
            }
        }

        public bool IsConnected()
        {
            return WebSocket.ReadyState == WebSocketState.Open;
        }

        public async Task<Dictionary<string, IPEndPoint>> GetEndpointsRequestAsync(string serverId, string profileId)
        {
            var requestId = Guid.NewGuid().ToString();

            RequestCompletionSourceList.Add(requestId, new TaskCompletionSource<object>());

            var getServerEndPointsRequest = new Dictionary<string, object>
            {
                { "requestId", requestId },
                { "requestType", "getEndPointsRequest" },
                { "serverId", serverId },
                { "profileId", profileId },
            };

            WebSocket.Send(JsonConvert.SerializeObject(getServerEndPointsRequest));

            var publicEndPoints = (Dictionary<string, IPEndPoint>)await RequestCompletionSourceList[requestId].Task;

            return publicEndPoints;
        }

        public async Task<bool> NatPunchRequestAsync(string serverId, string profileId, Dictionary<string, IPEndPoint> remoteEndPoints)
        {
            var requestId = Guid.NewGuid().ToString();

            RequestCompletionSourceList.Add(requestId, new TaskCompletionSource<object>());

            var natPunchRequest = new Dictionary<string, object>
            {
                { "requestId", requestId },
                { "requestType", "natPunchRequest" },
                { "serverId", serverId },
                { "profileId", profileId },
                { "publicEndPoints", PublicEndPoints.ToDictionary(x => x.Key, x => x.Value.ToString()) }
            };

            WebSocket.Send(JsonConvert.SerializeObject(natPunchRequest));

            await RequestCompletionSourceList[requestId].Task;

            if (remoteEndPoints.ContainsKey("stun"))
            {
                PunchNat(remoteEndPoints["stun"]);
            }

            return true;
        }

        public void PunchNat(IPEndPoint endPoint)
        {
            // bogus punch data
            NetDataWriter resp = new NetDataWriter();
            resp.Put(9999);

            EFT.UI.ConsoleScreen.Log($"Punching: {endPoint}");

            // send a couple of packets to punch a hole
            for (int i = 0; i < 10; i++)
            {
                NetManager.SendUnconnectedMessage(resp, endPoint);
            }
        }

        public async Task<bool> AddUpnpEndPoint(int localPort, int publicPort, int lifetime, string desc)
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(5000);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                var extIp = await device.GetExternalIPAsync();
                var externalIp = extIp.MapToIPv4();
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, localPort, publicPort, lifetime, desc));

                PublicEndPoints.Add("upnp", new IPEndPoint(externalIp, publicPort));

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"Warning: UPNP mapping failed: {ex.Message}");
                EFT.UI.ConsoleScreen.Log($"Warning: UPNP mapping failed: {ex.Message}");
            }

            return false;
        }

        public bool AddStunEndPoint(int port = 0)
        {
            bool success = false;
            var stunUdpClient = new UdpClient();
            try
            {
                var queryResult = new STUNQueryResult();

                if (port > 0)
                    stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                IPAddress stunIp = Array.Find(Dns.GetHostEntry("stun.l.google.com").AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                int stunPort = 19302;

                var stunEndPoint = new IPEndPoint(stunIp, stunPort);

                queryResult = STUNClient.Query(stunUdpClient.Client, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);
                //queryResult = STUNClient.Query(stunEndPoint, STUNQueryType.ExactNAT, true, NATTypeDetectionRFC.Rfc3489);

                success = queryResult.PublicEndPoint != null;
                if (success)
                {
                    PublicEndPoints.Add("stun", queryResult.PublicEndPoint);
                }
                else
                {
                    var msg = $"Warning: STUN query failed.";
                    Logger.LogInfo(msg);
                    EFT.UI.ConsoleScreen.Log(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"STUN Error: {ex.Message}");
                EFT.UI.ConsoleScreen.Log($"STUN Error: {ex.Message}");
            }
            finally
            {
                stunUdpClient.Client.Close();
            }

            return success;
        }

        private async Task<IPAddress?> GetExternalIPAddressByWebCall(string address)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 1);
                    string result = await client.GetStringAsync(address);
                    return IPAddress.Parse(result);
                }
            }
            catch { }

            return null;
        }

        public async Task<bool> AddThirdPartyIPEndpoint(int port)
        {
            string[] WebsitesToGetIPs = ["https://api.ipify.org/", "http://wtfismyip.com/text", "https://ipv4.icanhazip.com/"];
            foreach (var address in WebsitesToGetIPs)
            {
                var addr = await GetExternalIPAddressByWebCall(address);
                if (addr != null)
                {
                    PublicEndPoints.Add("external", new IPEndPoint(addr, port));
                    return true;
                }
            }
            return false;
        }

        public void AddEndPoint(string name, string ip, int port)
        {
            // NOTE(belette) probably fail loudly here instead?
            bool result = IPAddress.TryParse(ip, out var ipAddress);

            if (result)
                PublicEndPoints.Add(name, new IPEndPoint(ipAddress, port));
        }
    }

    public static class ExtensionMethods
    {
        public static IPEndPoint ToIPEndPoint(this string ipEndPoint)
        {
            var ipEndPointArr = ipEndPoint.Split(':');
            var ip = ipEndPointArr[0];
            var port = int.Parse(ipEndPointArr[1]);

            return new IPEndPoint(IPAddress.Parse(ip), port);
        }
    }
}
