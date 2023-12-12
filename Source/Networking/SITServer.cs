using Comfort.Common;
using EFT;
using LiteNetLib;
using LiteNetLib.Utils;
using StayInTarkov.Coop;
using StayInTarkov.Coop.NetworkPacket;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class SITServer : MonoBehaviour, INetEventListener, INetLogger
    {
        private LiteNetLib.NetManager _netServer;
        private NetPeer _ourPeer;
        private NetDataWriter _dataWriter;
        private NetDataReader _dataReader;

        public void Start()
        {
            NetDebug.Logger = this;
            _dataWriter = new NetDataWriter();
            _netServer = new LiteNetLib.NetManager(this)
            {
                BroadcastReceiveEnabled = true,
                UpdateTime = 15,
                AutoRecycle = true
            };
            _netServer.Start(5000);
            EFT.UI.ConsoleScreen.Log("Started SITServer");
        }

        void Update()
        {
            _netServer.PollEvents();
        }

        //void FixedUpdate()
        //{
        //    if (_ourPeer != null)
        //    {
        //        _dataWriter.Reset();
        //        _dataWriter.Put(_serverBall.transform.position.x);
        //        _ourPeer.Send(_dataWriter, DeliveryMethod.Sequenced);
        //    }
        //}

        void OnDestroy()
        {
            NetDebug.Logger = null;
            if (_netServer != null)
                _netServer.Stop();
        }

        public void SendPacket(byte[] data, DeliveryMethod deliveryMethod)
        {
            _ourPeer.Send(data, deliveryMethod);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] We have new peer " + peer.EndPoint);
            _ourPeer = peer;
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
            request.AcceptIfKey("sample_app");
        }

        public void OnPeerDisconnected(NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
            if (peer == _ourPeer)
                _ourPeer = null;
        }

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Debug.LogFormat(str, args);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            PlayerStatePacket pSP = new();
            pSP.Deserialize(reader);

            var asd = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            foreach ( var player in asd )
            {
                if (!player.IsYourPlayer)
                {
                    var cp = player as CoopPlayer;
                    cp.ApplyStatePacket(pSP);
                }
            }
        }
    }
}
