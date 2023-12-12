using Comfort.Common;
using EFT;
using LiteNetLib;
using LiteNetLib.Utils;
using StayInTarkov.Coop;
using StayInTarkov.Coop.NetworkPacket;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class SITServer : MonoBehaviour, INetEventListener, INetLogger
    {
        private LiteNetLib.NetManager _netServer;
        public NetDataWriter _dataWriter;

        public void Start()
        {
            NetDebug.Logger = this;
            _dataWriter = new NetDataWriter();
            _netServer = new LiteNetLib.NetManager(this)
            {
                BroadcastReceiveEnabled = true,
                UpdateTime = 15,
                AutoRecycle = true,
                IPv6Enabled = false
            };
            var ip = "127.0.0.1";
            _netServer.Start(IPAddress.Parse(ip), IPAddress.IPv6Any, 5000);
            EFT.UI.ConsoleScreen.Log("Started SITServer");
            NotificationManagerClass.DisplayMessageNotification($"Server started on {ip}.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.EntryPoint);
        }

        void Update()
        {
            _netServer.PollEvents();
        }

        void OnDestroy()
        {
            NetDebug.Logger = null;
            if (_netServer != null)
                _netServer.Stop();
        }

        public void SendData(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            _netServer.SendToAll(dataWriter, deliveryMethod);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] We have new peer " + peer.EndPoint);
            NotificationManagerClass.DisplayMessageNotification($"Peer {peer.Id} connected to server.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Friend);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] error " + socketErrorCode);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.Broadcast)
            {
                EFT.UI.ConsoleScreen.Log("[SERVER] Received discovery request. Send discovery response");
                NetDataWriter resp = new NetDataWriter();
                resp.Put(1);
                _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
            }
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(LiteNetLib.ConnectionRequest request)
        {
            request.AcceptIfKey("sit.core");
        }

        public void OnPeerDisconnected(NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        }

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Debug.LogFormat(str, args);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            PlayerStatePacket pSP = new();
            pSP.Deserialize(reader);

            var playerToApply = Singleton<GameWorld>.Instance.allAlivePlayersByID[pSP.ProfileId] as CoopPlayer;
            if (playerToApply != null && !playerToApply.IsYourPlayer)
            {
                playerToApply.ApplyStatePacket(pSP);
            }
        }
    }
}
