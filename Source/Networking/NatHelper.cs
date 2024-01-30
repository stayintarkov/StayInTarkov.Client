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
        public TaskCompletionSource<Dictionary<string, string>> NatTraversalCompletionSource;
        public Dictionary<string, string> PublicEndPoints = new Dictionary<string, string>();

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("P2P Connection Helper");

        public NatHelper(LiteNetLib.NetManager netManager)
        {
            NetManager = netManager;
        }

        public void Connect()
        {
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{PluginConfigSettings.Instance.CoopSettings.SITNatPunchHelperPort}/{MatchmakerAcceptPatches.Profile.ProfileId}?";

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
            try
            {
                JObject msgObj = JObject.Parse(message);

                if(msgObj.ContainsKey("getEndPointsRequest"))
                {
                    var profileId = msgObj["profileId"];
                    var publicEndPoints = msgObj["publicEndPoints"].ToObject<Dictionary<string, string>>();

                    if(publicEndPoints.ContainsKey("stun"))
                    {
                        var stunEndPointIp = publicEndPoints["stun"].Split(':')[0];
                        var stunEndPointPort = int.Parse(publicEndPoints["stun"].Split(':')[1]);
                        PunchNat(new IPEndPoint(IPAddress.Parse(stunEndPointIp), stunEndPointPort));
                    }

                    var getServerEndPointsResponse = new Dictionary<string, object>
                    {
                        { "getEndPointsResponse", true },
                        { "profileId", profileId },
                        { "publicEndPoints", PublicEndPoints }
                    };

                    WebSocket.Send(JsonConvert.SerializeObject(getServerEndPointsResponse));
                }

                if(msgObj.ContainsKey("getEndPointsResponse"))
                {
                    var publicEndPoints = msgObj["publicEndPoints"].ToObject<Dictionary<string, string>>();

                    if (publicEndPoints.ContainsKey("stun"))
                    {
                        var stunEndPointIp = publicEndPoints["stun"].Split(':')[0];
                        var stunEndPointPort = int.Parse(publicEndPoints["stun"].Split(':')[1]);
                        PunchNat(new IPEndPoint(IPAddress.Parse(stunEndPointIp), stunEndPointPort));
                    }

                    NatTraversalCompletionSource.SetResult(publicEndPoints);
                }
            }

            catch(Exception ex)
            {
                EFT.UI.ConsoleScreen.Log(ex.Message);
            }
        }

        public async Task<Dictionary<string, string>> GetEndpointsRequest(string serverId, string profileId)
        {
            NatTraversalCompletionSource = new TaskCompletionSource<Dictionary<string, string>>();

            if (PublicEndPoints != null && PublicEndPoints.Count > 0)
            {
                var getServerEndPointsRequest = new Dictionary<string, object>
                {
                    { "getEndPointsRequest", true },
                    { "serverId", serverId },
                    { "profileId", profileId },
                    { "publicEndPoints", PublicEndPoints }
                };

                WebSocket.Send(JsonConvert.SerializeObject(getServerEndPointsRequest));

                var publicEndPoints = await NatTraversalCompletionSource.Task;

                return publicEndPoints;
            }

            return null;
        }

        private void PunchNat(IPEndPoint endPoint)
        {
            // bogus punch data
            NetDataWriter resp = new NetDataWriter();
            resp.Put(9999);

            // send a couple of packets to punch a hole
            for (int i = 0; i < 10; i++)
            {
                NetManager.SendUnconnectedMessage(resp, endPoint);
            }
        }

        public async Task AddUpnpMap(int port, int lifetime, string desc)
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
            }
            catch (Exception ex)
            {
                EFT.UI.ConsoleScreen.Log(ex.Message);
            }
        }

        public void AddStunEndPoint(int port)
        {
            var queryResult = new STUNQueryResult();
            var stunUdpClient = new UdpClient(AddressFamily.InterNetwork);

            try
            {
                stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                IPAddress stunIp = Array.Find(Dns.GetHostEntry("stun.l.google.com").AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                int stunPort = 19302;

                var stunEndPoint = new IPEndPoint(stunIp, stunPort);

                queryResult = STUNClient.Query(stunUdpClient.Client, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);
                //var queryResult = STUNClient.Query(stunEndPoint, STUNQueryType.ExactNAT, true, NATTypeDetectionRFC.Rfc3489);

                PublicEndPoints.Add("stun", $"{queryResult.PublicEndPoint.Address}:{queryResult.PublicEndPoint.Port}");

            }
            catch (Exception ex)
            {
                EFT.UI.ConsoleScreen.Log(ex.Message);
            }

            stunUdpClient.Client.Close();
        }

        public void AddPortForwardEndPoint(string externalIp, int port)
        {
            if (externalIp != null && !string.IsNullOrEmpty(externalIp))
            {
                PublicEndPoints.Add("portforward", $"{externalIp}:{port}");
            }
        }
    }
}
