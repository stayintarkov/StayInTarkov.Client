using BepInEx.Logging;
using Comfort.Common;
using EFT;
using LiteNetLib;
using LiteNetLib.Utils;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Players;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking
{
    public class GameServerUDP : MonoBehaviour, INetEventListener, INetLogger
    {
        public NatHelper _natHelper;
        private LiteNetLib.NetManager _netServer;
        public NetPacketProcessor _packetProcessor = new();
        private NetDataWriter _dataWriter = new();
        public CoopPlayer MyPlayer => Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;
        public List<string> PlayersMissing = [];
        private SITGameComponent CoopGameComponent { get; set; }
        public LiteNetLib.NetManager NetServer
        {
            get
            {
                return _netServer;
            }
        }

        private ManualLogSource Logger { get; set; }

        void Awake()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<SITGameComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GameServerUDP));
        }

        public async void Start()
        {
            NetDebug.Logger = this;

            _packetProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
            _packetProcessor.RegisterNestedType(Vector2Utils.Serialize, Vector2Utils.Deserialize);
            _packetProcessor.RegisterNestedType(PhysicalUtils.Serialize, PhysicalUtils.Deserialize);

            //_packetProcessor.SubscribeNetSerializable<PlayerStatePacket, NetPeer>(OnPlayerStatePacketReceived);
            //_packetProcessor.SubscribeNetSerializable<GameTimerPacket, NetPeer>(OnGameTimerPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<WeatherPacket, NetPeer>(OnWeatherPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<WeaponPacket, NetPeer>(OnWeaponPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<HealthPacket, NetPeer>(OnHealthPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<InventoryPacket, NetPeer>(OnInventoryPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<CommonPlayerPacket, NetPeer>(OnCommonPlayerPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<AllCharacterRequestPacket, NetPeer>(OnAllCharacterRequestPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<InformationPacket, NetPeer>(OnInformationPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<PlayerProceedPacket, NetPeer>(OnPlayerProceedPacket);

            _netServer = new LiteNetLib.NetManager(this)
            {
                BroadcastReceiveEnabled = true,
                UpdateTime = 15,
                AutoRecycle = true,
                IPv6Enabled = false,
                EnableStatistics = true,
                NatPunchEnabled = false
            };

            // ===============================================================
            // HERE BE DRAGONS.
            // Make sure you understand NAT punching, UPnP and port-forwarding
            // if you plan on changing this code
            // ===============================================================

            var localPort = PluginConfigSettings.Instance.CoopSettings.UdpServerLocalPort;

            var msg = $"Connecting to NAT Helper...";
            EFT.UI.ConsoleScreen.Log(msg);
            Logger.LogDebug(msg);

            _natHelper = new NatHelper(_netServer, SITMatchmaking.Profile.ProfileId);
            _natHelper.Connect();

            msg = $"Setting up Public Endpoints...";
            EFT.UI.ConsoleScreen.Log(msg);
            Logger.LogInfo(msg);

            // Use explicitly set ip/port if possible, otherwise use UPnP, then STUN, then 3rd-party
            if (!string.IsNullOrWhiteSpace(SITMatchmaking.PublicIPAddress) && SITMatchmaking.PublicPort != 0)
            {
                // Port forwarding
                _natHelper.AddEndPoint("explicit", SITMatchmaking.PublicIPAddress, SITMatchmaking.PublicPort);
            }
            else
            {
                // UPnP
                var upnpResult = await _natHelper.AddUpnpEndPoint(localPort, SITMatchmaking.PublicPort, 900, "sit udp");

                // Only do STUN (nat punch) if UPnP failed
                if (!upnpResult)
                {
                    _natHelper.AddStunEndPoint(ref localPort);
                }
            }

            if (_natHelper.PublicEndPoints.IsNullOrEmpty())
            {
                var errmsg = $"Could not find any available public endpoints, please set one explicitly in your config.";
                Logger.LogError(errmsg);
                EFT.UI.ConsoleScreen.LogError(errmsg);
                NotificationManagerClass.DisplayMessageNotification(errmsg,
                    EFT.Communications.ENotificationDurationType.Long, EFT.Communications.ENotificationIconType.Alert);
                return;
            }

            // Listen locally
            var localIPv4 = IPAddress.Parse(PluginConfigSettings.Instance.CoopSettings.UdpServerLocalIPv4);
            var localIPv6 = IPAddress.Parse(PluginConfigSettings.Instance.CoopSettings.UdpServerLocalIPv6);
            _netServer.Start(localIPv4, localIPv6, localPort);

            msg = $"Server listening on {localIPv4}:{_netServer.LocalPort} and [{localIPv6}]:{_netServer.LocalPort}.";
            Logger.LogInfo(msg);
            EFT.UI.ConsoleScreen.Log(msg);
            NotificationManagerClass.DisplayMessageNotification(msg,
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.EntryPoint);

            msg = $"Registered endpoints ${string.Join("\n", _natHelper.PublicEndPoints)}";
            Logger.LogInfo(msg);
            EFT.UI.ConsoleScreen.Log(msg);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            //Logger.LogInfo("[Server] OnNetworkReceive");
            var bytes = reader.GetRemainingBytes();
            _netServer.SendToAll(bytes, deliveryMethod);
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

            if(_natHelper != null)
            {
                _natHelper.Close();
            }
        }

        public void SendDataToAll<T>(NetDataWriter writer, ref T packet, DeliveryMethod deliveryMethod, NetPeer peer = null) where T : INetSerializable
        {
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            if (peer != null)
                _netServer.SendToAll(writer, deliveryMethod, peer);
            else
                _netServer.SendToAll(writer, deliveryMethod);
        }

        public void SendDataToPeer<T>(NetPeer peer, NetDataWriter writer, ref T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            peer.Send(writer, deliveryMethod);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log($"[SERVER] {nameof(OnPeerConnected)} {peer.EndPoint} connected to server.");

            NotificationManagerClass.DisplayMessageNotification($"Peer {peer.EndPoint} connected to server.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Friend);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            EFT.UI.ConsoleScreen.LogError("[SERVER] error " + socketErrorCode);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.Broadcast && reader.GetInt() == 1)
            {
                EFT.UI.ConsoleScreen.Log($"[SERVER] Received discovery request. Send discovery response to {remoteEndPoint}");
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