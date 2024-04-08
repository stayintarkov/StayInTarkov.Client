using BepInEx.Logging;
using Comfort.Common;
using EFT;
using LiteNetLib;
using LiteNetLib.Utils;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Players;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

            EFT.UI.ConsoleScreen.Log($"Connecting to Nat Helper...");
            
            _natHelper = new NatHelper(_netServer);
            _natHelper.Connect();

            EFT.UI.ConsoleScreen.Log($"Creating Public Endpoints...");
            
            // UPNP
            var upnpResult = await _natHelper.AddUpnpEndPoint(SITMatchmaking.Port, 900, "sit.core");

            // Only do STUN (nat punch) if UPNP failed.
            if(!upnpResult)
                _natHelper.AddStunEndPoint(SITMatchmaking.Port);

            // External (port forwarding)
            _natHelper.AddEndPoint("external", SITIPAddressManager.SITIPAddresses.ExternalAddresses.IPAddressV4, SITMatchmaking.Port);

            _netServer.Start(SITMatchmaking.Port);

            Logger.LogDebug($"Server started on port {_netServer.LocalPort}.");
            EFT.UI.ConsoleScreen.Log($"Server started on port {_netServer.LocalPort}.");
            NotificationManagerClass.DisplayMessageNotification($"Server started on port {_netServer.LocalPort}.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.EntryPoint);
        }

        //private void OnPlayerProceedPacket(PlayerProceedPacket packet, NetPeer peer)
        //{
        //    Logger.LogInfo("[Server] OnPlayerProceedPacket");
        //    Logger.LogInfo(packet.ToJson());
        //}

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            //Logger.LogInfo("[Server] OnNetworkReceive");
            var bytes = reader.GetRemainingBytes();
            SITGameServerClientDataProcessing.ProcessPacketBytes(bytes, Encoding.UTF8.GetString(bytes));
            _netServer.SendToAll(bytes, deliveryMethod);

#if DEBUG

            if (_netServer.Statistics.PacketLossPercent > 0)
            {
                Logger.LogError($"Packet Loss {_netServer.Statistics.PacketLossPercent}%");
            }

#endif
        }


        //private void OnInformationPacketReceived(InformationPacket packet, NetPeer peer)
        //{
        //    InformationPacket respondPackage = new(false)
        //    {
        //        NumberOfPlayers = _netServer.ConnectedPeersCount
        //    };

        //    _dataWriter.Reset();
        //    SendDataToPeer(peer, _dataWriter, ref respondPackage, DeliveryMethod.ReliableUnordered);
        //}

        //private void OnAllCharacterRequestPacketReceived(AllCharacterRequestPacket packet, NetPeer peer)
        //{
        //    // This method needs to be refined. For some reason the ping-pong has to be run twice for it to work on the host?
        //    if (packet.IsRequest)
        //    {
        //        foreach (var player in CoopGameComponent.Players.Values)
        //        {
        //            if (player.ProfileId == packet.ProfileId)
        //                continue;

        //            if (packet.Characters.Contains(player.ProfileId))
        //                continue;

        //            AllCharacterRequestPacket requestPacket = new(player.ProfileId)
        //            {
        //                IsRequest = false,
        //                PlayerInfo = new()
        //                {
        //                    Profile = player.Profile
        //                },
        //                IsAlive = player.ActiveHealthController.IsAlive,
        //                Position = player.Transform.position
        //            };
        //            _dataWriter.Reset();
        //            SendDataToPeer(peer, _dataWriter, ref requestPacket, DeliveryMethod.ReliableUnordered);
        //        }
        //    }
        //    if (!Players.ContainsKey(packet.ProfileId) && !PlayersMissing.Contains(packet.ProfileId))
        //    {
        //        PlayersMissing.Add(packet.ProfileId);
        //        EFT.UI.ConsoleScreen.Log($"Requesting missing player from server.");
        //        AllCharacterRequestPacket requestPacket = new(MyPlayer.ProfileId);
        //        _dataWriter.Reset();
        //        SendDataToPeer(peer, _dataWriter, ref requestPacket, DeliveryMethod.ReliableUnordered);
        //    }
        //    if (!packet.IsRequest && PlayersMissing.Contains(packet.ProfileId))
        //    {
        //        EFT.UI.ConsoleScreen.Log($"Received CharacterRequest from client: ProfileID: {packet.PlayerInfo.Profile.ProfileId}, Nickname: {packet.PlayerInfo.Profile.Nickname}");
        //        if (packet.ProfileId != MyPlayer.ProfileId)
        //        {
        //            if (!CoopGameComponent.PlayersToSpawn.ContainsKey(packet.PlayerInfo.Profile.ProfileId))
        //                CoopGameComponent.PlayersToSpawn.TryAdd(packet.PlayerInfo.Profile.ProfileId, ESpawnState.None);
        //            if (!CoopGameComponent.PlayersToSpawnProfiles.ContainsKey(packet.PlayerInfo.Profile.ProfileId))
        //                CoopGameComponent.PlayersToSpawnProfiles.Add(packet.PlayerInfo.Profile.ProfileId, packet.PlayerInfo.Profile);

        //            //CoopGameComponent.QueueProfile(packet.PlayerInfo.Profile, new Vector3(packet.Position.x, packet.Position.y + 0.5f, packet.Position.y), packet.IsAlive);
        //            PlayersMissing.Remove(packet.ProfileId);
        //        }
        //    }
        //}
        //private void OnCommonPlayerPacketReceived(CommonPlayerPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    _dataWriter.Reset();
        //    SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered, peer);

        //    var playerToApply = Players[packet.ProfileId] as CoopPlayerClient;
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        //playerToApply.CommonPlayerPackets.Enqueue(packet);
        //    }
        //}
        //private void OnInventoryPacketReceived(ItemPlayerPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    _dataWriter.Reset();
        //    SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered, peer);

        //    var playerToApply = Players[packet.ProfileId] as CoopPlayerClient;
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        //playerToApply.InventoryPackets.Enqueue(packet);
        //    }
        //}
        //private void OnHealthPacketReceived(HealthPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    _dataWriter.Reset();
        //    SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered, peer);

        //    var playerToApply = Players[packet.ProfileId] as CoopPlayerClient;
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        //playerToApply.HealthPackets.Enqueue(packet);
        //    }
        //}
        //private void OnWeaponPacketReceived(WeaponPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    _dataWriter.Reset();
        //    SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered, peer);

        //    var playerToApply = Players[packet.ProfileId] as CoopPlayerClient;
        //    if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
        //    {
        //        //playerToApply.FirearmPackets.Enqueue(packet);
        //    }
        //}
        //private void OnWeatherPacketReceived(WeatherPacket packet, NetPeer peer)
        //{
        //    if (!packet.IsRequest)
        //        return;

        //    var weatherController = WeatherController.Instance;
        //    if (weatherController != null)
        //    {
        //        WeatherPacket weatherPacket = new();
        //        if (weatherController.CloudsController != null)
        //            weatherPacket.CloudDensity = weatherController.CloudsController.Density;

        //        var weatherCurve = weatherController.WeatherCurve;
        //        if (weatherCurve != null)
        //        {
        //            weatherPacket.Fog = weatherCurve.Fog;
        //            weatherPacket.LightningThunderProbability = weatherCurve.LightningThunderProbability;
        //            weatherPacket.Rain = weatherCurve.Rain;
        //            weatherPacket.Temperature = weatherCurve.Temperature;
        //            weatherPacket.WindX = weatherCurve.Wind.x;
        //            weatherPacket.WindY = weatherCurve.Wind.y;
        //            weatherPacket.TopWindX = weatherCurve.TopWind.x;
        //            weatherPacket.TopWindY = weatherCurve.TopWind.y;
        //        }

        //        _dataWriter.Reset();
        //        SendDataToPeer(peer, _dataWriter, ref weatherPacket, DeliveryMethod.ReliableOrdered);
        //    }
        //}
        //private void OnGameTimerPacketReceived(GameTimerPacket packet, NetPeer peer)
        //{
        //    if (!packet.IsRequest)
        //        return;

        //    var game = (CoopGame)Singleton<AbstractGame>.Instance;
        //    if (game != null)
        //    {
        //        GameTimerPacket gameTimerPacket = new(false, (game.GameTimer.SessionTime - game.GameTimer.PastTime).Value.Ticks);
        //        _dataWriter.Reset();
        //        SendDataToPeer(peer, _dataWriter, ref gameTimerPacket, DeliveryMethod.ReliableOrdered);
        //    }
        //    else
        //    {
        //        EFT.UI.ConsoleScreen.Log("OnGameTimerPacketReceived: Game was null!");
        //    }
        //}

        //private void OnPlayerStatePacketReceived(PlayerStatePacket packet, NetPeer peer)
        //{

        //    Logger.LogInfo($"{nameof(OnPlayerStatePacketReceived)}");

        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    _dataWriter.Reset();
        //    SendDataToAll(_dataWriter, ref packet, DeliveryMethod.ReliableOrdered, peer);

        //    var playerToApply = Players[packet.ProfileId] as CoopPlayerClient;
        //    if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
        //    {
        //        playerToApply.NewState = packet;
        //    }
        //}

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