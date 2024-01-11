using BepInEx.Logging;
using LiteNetLib.Utils;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StayInTarkov.Networking
{
    public class NatTraversalRequest
    {
        private P2PConnectionHelper P2PConnectionHelper { get; set; }
        private TaskCompletionSource<IPEndPoint> NatTraversalCompletionSource;
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("NAT Traversal Request");

        public NatTraversalRequest(P2PConnectionHelper p2PConnectionHelper) 
        { 
            P2PConnectionHelper = p2PConnectionHelper;

            P2PConnectionHelper.WebSocket.OnMessage += WebSocket_OnMessage;
            P2PConnectionHelper.WebSocket.OnError += WebSocket_OnError;
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
            P2PConnectionHelper.WebSocket.Close();
        }

        private void ProcessMessage(string message)
        {
            Logger.LogInfo("received message: " + message);

            var messageSplit = message.Split(':');

            if (messageSplit[0] == "punch_response")
            {
                var serverIp = messageSplit[1];
                var serverPort = messageSplit[2];

                var endPoint = new IPEndPoint(IPAddress.Parse(serverIp), int.Parse(serverPort));

                P2PConnectionHelper.NetManager.Start(PluginConfigSettings.Instance.CoopSettings.SITUdpPort);
                P2PConnectionHelper.PunchNat(endPoint);

                NatTraversalCompletionSource.SetResult(endPoint);
            }
        }

        public async Task<IPEndPoint> NatPunchRequestAsync(string serverId, string profileId)
        {
            NatTraversalCompletionSource = new TaskCompletionSource<IPEndPoint>();

            // perform STUN query to open a public endpoint and punch it later.
            var stunEndPoint = STUNHelper.OpenPublicEndpoint(PluginConfigSettings.Instance.CoopSettings.SITUdpPort);

            if (stunEndPoint != null)
            {
                // punch_request:serverId:profileId:publicIp:publicPort
                string punchRequestPacket = $"punch_request:{serverId}:{profileId}:{stunEndPoint.Address}:{stunEndPoint.Port}";

                P2PConnectionHelper.WebSocket.Send(punchRequestPacket);

                var endPoint = await NatTraversalCompletionSource.Task;

                return endPoint;
            }

            return null;
        }

        public async Task<IPEndPoint> UpnpRequest(string serverId, string profileId)
        {
            var endPoint = await NatTraversalCompletionSource.Task;

            return endPoint;
        }
    }

    public class P2PConnectionHelper
    {
        public LiteNetLib.NetManager NetManager;
        public WebSocket WebSocket { get; set; }

        public IPEndPoint PublicEndPoint { get; set; }

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("P2P Connection Helper");

        public P2PConnectionHelper(LiteNetLib.NetManager netManager)
        {
            NetManager = netManager;
        }

        public void Connect()
        {
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{PluginConfigSettings.Instance.CoopSettings.SITP2PHelperPort}/{MatchmakerAcceptPatches.Profile.ProfileId}?";

            WebSocket = new WebSocket(wsUrl);
            WebSocket.WaitTime = TimeSpan.FromMinutes(1);
            WebSocket.EmitOnPing = true;
            WebSocket.Connect();

            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
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
            Logger.LogInfo("received message: " + message);

            var messageSplit = message.Split(':');

            if (messageSplit[0] == "punch_request")
            {
                var profileId = messageSplit[1];
                var ipToPunch = messageSplit[2];
                var portToPunch = messageSplit[3];

                if(PublicEndPoint == null)
                {
                    PublicEndPoint = STUNHelper.OpenPublicEndpoint(PluginConfigSettings.Instance.CoopSettings.SITUdpPort);
                }

                string punchResponsePacket = $"punch_response:{profileId}:{PublicEndPoint.Address}:{PublicEndPoint.Port}";
                WebSocket.Send(punchResponsePacket);

                NetManager.Start(PluginConfigSettings.Instance.CoopSettings.SITUdpPort);

                PunchNat(new IPEndPoint(IPAddress.Parse(ipToPunch), int.Parse(portToPunch)));
            }
        }

        public void PunchNat(IPEndPoint endPoint)
        {
            // bogus punch data
            NetDataWriter resp = new NetDataWriter();
            resp.Put(9999);

            Logger.LogInfo($"Punching: {endPoint.Address.ToString()}:{endPoint.Port}");

            // send a couple of packets to punch a hole
            for (int i = 0; i < 10; i++)
            {
                NetManager.SendUnconnectedMessage(resp, endPoint);
            }
        }
    }
}
