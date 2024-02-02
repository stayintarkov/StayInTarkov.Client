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
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StayInTarkov.Networking
{
    public enum ServerType
    {
        Relay,
        P2P
    }

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
        public Dictionary<string, string> PublicEndPoints = new Dictionary<string, string>();

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Net Helper");

        public NatHelper(LiteNetLib.NetManager netManager)
        {
            NetManager = netManager;
        }

        public NatHelper()
        {

        }

        public void Connect()
        {
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{PluginConfigSettings.Instance.CoopSettings.SITNatHelperPort}/{MatchmakerAcceptPatches.Profile.ProfileId}?";

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

            if(msgObj.ContainsKey("requestId") && msgObj.ContainsKey("requestType"))
            {
                var requestId = msgObj["requestId"].ToString();
                var requestType = msgObj["requestType"].ToString();
                var profileId = msgObj["profileId"].ToString();

                if (requestType == "getEndPointsRequest")
                {
                    var getServerEndPointsResponse = new Dictionary<string, object>
                    {
                        { "requestId", requestId },
                        { "requestType", "getEndPointsResponse" },
                        { "profileId", profileId },
                        { "publicEndPoints", PublicEndPoints }
                    };

                    WebSocket.Send(JsonConvert.SerializeObject(getServerEndPointsResponse));
                }

                if (requestType == "natPunchRequest")
                {
                    var publicEndPoints = msgObj["publicEndPoints"].ToObject<Dictionary<string, string>>();

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
                    var publicEndPoints = msgObj["publicEndPoints"].ToObject<Dictionary<string, string>>();

                    if(RequestCompletionSourceList.ContainsKey(requestId))
                        RequestCompletionSourceList[requestId].SetResult(publicEndPoints);
                }

                if (requestType == "natPunchResponse")
                {
                    if (RequestCompletionSourceList.ContainsKey(requestId))
                        RequestCompletionSourceList[requestId].SetResult(true);
                }
            }
        }

        public async Task<Dictionary<string, string>> GetEndpointsRequestAsync(string serverId, string profileId)
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

            var publicEndPoints = (Dictionary<string, string>)await RequestCompletionSourceList[requestId].Task;

            return publicEndPoints;
        }

        public async Task<bool> NatPunchRequestAsync(string serverId, string profileId, Dictionary<string, string> remoteEndPoints)
        {
            var requestId = Guid.NewGuid().ToString();

            RequestCompletionSourceList.Add(requestId, new TaskCompletionSource<object>());

            var natPunchRequest = new Dictionary<string, object>
            {
                { "requestId", requestId },
                { "requestType", "natPunchRequest" },
                { "serverId", serverId },
                { "profileId", profileId },
                { "publicEndPoints", PublicEndPoints }
            };

            WebSocket.Send(JsonConvert.SerializeObject(natPunchRequest));

            await RequestCompletionSourceList[requestId].Task;

            if (remoteEndPoints.ContainsKey("stun"))
            {
                PunchNat(remoteEndPoints["stun"]);
            }

            return true;
        }

        public void PunchNat(string endPoint)
        {
            var endPointArr = endPoint.Split(':');
            var stunEndPointIp = endPointArr[0];
            var stunEndPointPort = int.Parse(endPointArr[1]);

            PunchNat(new IPEndPoint(IPAddress.Parse(stunEndPointIp), stunEndPointPort));
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

        public async Task<bool> AddUpnpEndPoint(int port, int lifetime, string desc)
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(5000);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                var extIp = await device.GetExternalIPAsync();
                var externalIp = extIp.MapToIPv4();
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port, port, lifetime, desc));

                PublicEndPoints.Add("upnp", $"{externalIp}:{port}");

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"Warning: UPNP mapping failed.");
                EFT.UI.ConsoleScreen.Log($"Warning: UPNP mapping failed.");
            }

            return false;
        }

        public void AddStunEndPoint(int port)
        {
            try
            {
                var queryResult = new STUNQueryResult();
                var stunUdpClient = new UdpClient();

                stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                IPAddress stunIp = Array.Find(Dns.GetHostEntry("stun.l.google.com").AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                int stunPort = 19302;

                var stunEndPoint = new IPEndPoint(stunIp, stunPort);

                queryResult = STUNClient.Query(stunUdpClient.Client, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);
                //var queryResult = STUNClient.Query(stunEndPoint, STUNQueryType.ExactNAT, true, NATTypeDetectionRFC.Rfc3489);

                if (queryResult.PublicEndPoint != null)
                {
                    PublicEndPoints.Add("stun", $"{queryResult.PublicEndPoint.Address}:{queryResult.PublicEndPoint.Port}");
                }
                else
                {
                    Logger.LogInfo($"Warning: STUN query failed.");
                    EFT.UI.ConsoleScreen.Log($"Warning: STUN query failed.");
                }

                stunUdpClient.Client.Close();
            }
            catch(Exception ex)
            {

            }
        }

        public void AddEndPoint(string name, string ip, int port)
        {
            if (ip != null && !string.IsNullOrEmpty(ip))
            {
                PublicEndPoints.Add(name, $"{ip}:{port}");
            }
        }
    }
}
