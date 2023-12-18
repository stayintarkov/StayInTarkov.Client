using Comfort.Common;
using EFT;
using EFT.Weather;
using LiteNetLib;
using LiteNetLib.Utils;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Networking.Packets;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking
{
    public class SITServer : MonoBehaviour, INetEventListener, INetLogger
    {
        private LiteNetLib.NetManager _netServer;
        public NetPacketProcessor _packetProcessor = new();
        private NetDataWriter _dataWriter = new();

        public ConcurrentDictionary<string, EFT.Player> Players => CoopGameComponent.Players;
        private CoopGameComponent CoopGameComponent { get; set; }

        public void Start()
        {
            NetDebug.Logger = this;

            _packetProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
            _packetProcessor.RegisterNestedType(Vector2Utils.Serialize, Vector2Utils.Deserialize);
            _packetProcessor.RegisterNestedType(PhysicalUtils.Serialize, PhysicalUtils.Deserialize);

            _packetProcessor.SubscribeNetSerializable<PlayerStatePacket, NetPeer>(OnPlayerStatePacketReceived);
            _packetProcessor.SubscribeNetSerializable<GameTimerPacket, NetPeer>(OnGameTimerPacketReceived);
            _packetProcessor.SubscribeNetSerializable<WeatherPacket, NetPeer>(OnWeatherPacketReceived);
            _packetProcessor.SubscribeNetSerializable<WeaponPacket, NetPeer>(OnWeaponPacketReceived);
            _packetProcessor.SubscribeNetSerializable<HealthPacket, NetPeer>(OnHealthPacketReceived);
            _packetProcessor.SubscribeNetSerializable<InventoryPacket, NetPeer>(OnInventoryPacketReceived);

            _netServer = new LiteNetLib.NetManager(this)
            {
                BroadcastReceiveEnabled = true,
                UpdateTime = 15,
                AutoRecycle = true,
                IPv6Enabled = false
            };

            _netServer.Start(PluginConfigSettings.Instance.CoopSettings.SITGamePlayPort);

            EFT.UI.ConsoleScreen.Log("Started SITServer");
            NotificationManagerClass.DisplayMessageNotification($"Server started on port {_netServer.LocalPort}.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.EntryPoint);
        }

        private void OnInventoryPacketReceived(InventoryPacket packet, NetPeer peer)
        {
            if (!Players.ContainsKey(packet.ProfileId))
                return;

            var playerToApply = Players[packet.ProfileId] as CoopPlayer;
            if (playerToApply != default && playerToApply != null)
            {
                playerToApply.InventoryPackets.Enqueue(packet);
            }
        }

        private void OnHealthPacketReceived(HealthPacket packet, NetPeer peer)
        {
            if (!Players.ContainsKey(packet.ProfileId))
                return;

            var playerToApply = Players[packet.ProfileId] as CoopPlayer;
            if (playerToApply != default && playerToApply != null)
            {
                playerToApply.HealthPackets.Enqueue(packet);
            }
        }

        private void OnWeaponPacketReceived(WeaponPacket packet, NetPeer peer)
        {
            if (!Players.ContainsKey(packet.ProfileId))
                return;

            _dataWriter.Reset();
            SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered);

            var playerToApply = Players[packet.ProfileId] as CoopPlayer;
            if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
            {
                playerToApply.FirearmPackets.Enqueue(packet);
            }
        }

        private void OnWeatherPacketReceived(WeatherPacket packet, NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log($"Received weather synchronization packet. IsRequest: {packet.IsRequest}");
            if (!packet.IsRequest)
                return;

            var weatherController = WeatherController.Instance;
            if (weatherController != null)
            {
                WeatherPacket weatherPacket = new();
                if (weatherController.CloudsController != null)
                    weatherPacket.CloudDensity = weatherController.CloudsController.Density;

                var weatherCurve = weatherController.WeatherCurve;
                if (weatherCurve != null)
                {
                    weatherPacket.Fog = weatherCurve.Fog;
                    weatherPacket.LightningThunderProbability = weatherCurve.LightningThunderProbability;
                    weatherPacket.Rain = weatherCurve.Rain;
                    weatherPacket.Temperature = weatherCurve.Temperature;
                    weatherPacket.WindX = weatherCurve.Wind.x;
                    weatherPacket.WindY = weatherCurve.Wind.y;
                    weatherPacket.TopWindX = weatherCurve.TopWind.x;
                    weatherPacket.TopWindY = weatherCurve.TopWind.y;
                }

                _dataWriter.Reset();
                SendDataToPeer(peer, _dataWriter, ref weatherPacket, DeliveryMethod.ReliableOrdered);
            }
        }

        private void OnGameTimerPacketReceived(GameTimerPacket packet, NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log($"Received game timer synchronization packet. IsRequest: {packet.IsRequest}");
            if (!packet.IsRequest)
                return;

            var game = (CoopGame)Singleton<AbstractGame>.Instance;
            if (game != null)
            {
                GameTimerPacket gameTimerPacket = new(false, (game.GameTimer.SessionTime - game.GameTimer.PastTime).Value.Ticks);
                _dataWriter.Reset();
                SendDataToPeer(peer, _dataWriter, ref gameTimerPacket, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                EFT.UI.ConsoleScreen.Log("OnGameTimerPacketReceived: Game was null!");
            }
        }

        private void OnPlayerStatePacketReceived(PlayerStatePacket packet, NetPeer peer)
        {
            if (!Players.ContainsKey(packet.ProfileId))
                return;

            var playerToApply = Players[packet.ProfileId] as CoopPlayer;
            if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
            {
                playerToApply.NewState = packet;
            }
        }

        public void Awake()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
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

        public void SendDataToAll<T>(NetDataWriter writer, ref T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            _netServer.SendToAll(writer, deliveryMethod);
        }

        public void SendDataToPeer<T>(NetPeer peer, NetDataWriter writer, ref T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            peer.Send(writer, deliveryMethod);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            NotificationManagerClass.DisplayMessageNotification($"Peer {peer.EndPoint} connected to server.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Friend);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            EFT.UI.ConsoleScreen.Log("[SERVER] error " + socketErrorCode);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
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
            _packetProcessor.ReadAllPackets(reader, peer);
        }
    }
}
