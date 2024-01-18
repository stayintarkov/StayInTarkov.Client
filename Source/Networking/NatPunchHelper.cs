using BepInEx.Logging;
using LiteNetLib.Utils;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StayInTarkov.Networking
{
    public class NatPunchHelper
    {
        public LiteNetLib.NetManager NetManager;
        public WebSocket WebSocket { get; set; }
        public IPEndPoint PublicEndPoint { get; private set; }
        private TaskCompletionSource<IPEndPoint> NatTraversalCompletionSource;

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("P2P Connection Helper");

        public NatPunchHelper(LiteNetLib.NetManager netManager)
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
            Logger.LogInfo("received message: " + message);

            var msgSplit = message.Split(':');

            if (msgSplit[0] == "punch_request")
            {
                var profileId = msgSplit[1];
                var ipToPunch = msgSplit[2];
                var portToPunch = msgSplit[3];

                PunchNat(new IPEndPoint(IPAddress.Parse(ipToPunch), int.Parse(portToPunch)));

                string punchResponsePacket = $"punch_response:{profileId}:{PublicEndPoint.Address}:{PublicEndPoint.Port}";
                WebSocket.Send(punchResponsePacket);
            }

            if (msgSplit[0] == "punch_response")
            {
                var serverIp = msgSplit[1];
                var serverPort = msgSplit[2];

                var endPoint = new IPEndPoint(IPAddress.Parse(serverIp), int.Parse(serverPort));

                PunchNat(endPoint);

                NatTraversalCompletionSource.SetResult(endPoint);
            }
        }

        public bool CreatePublicEndPoint(int localPort)
        {
            if (STUNHelper.Query(localPort, out STUNQueryResult stunQueryResult))
            {
                PublicEndPoint = stunQueryResult.PublicEndPoint;
                return true;
            }

            return false;
        }

        public async Task<IPEndPoint> NatPunchRequestAsync(string serverId, string profileId)
        {
            NatTraversalCompletionSource = new TaskCompletionSource<IPEndPoint>();

            if (PublicEndPoint != null)
            {
                // punch_request:serverId:profileId:publicIp:publicPort
                string punchRequestPacket = $"punch_request:{serverId}:{profileId}:{PublicEndPoint.Address}:{PublicEndPoint.Port}";

                WebSocket.Send(punchRequestPacket);

                var endPoint = await NatTraversalCompletionSource.Task;

                return endPoint;
            }

            return null;
        }

        private void PunchNat(IPEndPoint endPoint)
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
