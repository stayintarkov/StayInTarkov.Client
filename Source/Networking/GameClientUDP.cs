using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Sirenix.Utilities;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Players;
//using StayInTarkov.Coop.Players;
//using StayInTarkov.Networking.Packets;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityStandardAssets.Water;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking
{
    public class GameClientUDP : MonoBehaviour, INetEventListener, IGameClient
    {
        private LiteNetLib.NetManager _netClient;
        private P2PConnectionHelper _p2pConnectionHelper;
        private NetDataWriter _dataWriter = new();
        public CoopPlayer MyPlayer { get; set; }
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;
        private CoopGameComponent CoopGameComponent { get; set; }
        public NetPacketProcessor _packetProcessor = new();
        public int Ping = 0;
        public int ConnectedClients = 0;

        private ManualLogSource Logger { get; set; }

        void Awake()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GameClientUDP));
        }

        public void Start()
        {
            _packetProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
            _packetProcessor.RegisterNestedType(Vector2Utils.Serialize, Vector2Utils.Deserialize);
            _packetProcessor.RegisterNestedType(PhysicalUtils.Serialize, PhysicalUtils.Deserialize);

            //_packetProcessor.SubscribeNetSerializable<PlayerStatePacket, NetPeer>(OnPlayerStatePacketReceived);
            //_packetProcessor.SubscribeNetSerializable<GameTimerPacket, NetPeer>(OnGameTimerPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<WeatherPacket, NetPeer>(OnWeatherPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<WeaponPacket, NetPeer>(OnFirearmControllerPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<HealthPacket, NetPeer>(OnHealthPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<InventoryPacket, NetPeer>(OnInventoryPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<CommonPlayerPacket, NetPeer>(OnCommonPlayerPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<AllCharacterRequestPacket, NetPeer>(OnAllCharacterRequestPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<InformationPacket, NetPeer>(OnInformationPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<AirdropPacket, NetPeer>(OnAirdropPacketReceived);
            //_packetProcessor.SubscribeNetSerializable<AirdropLootPacket, NetPeer>(OnAirdropLootPacketReceived);

            _netClient = new LiteNetLib.NetManager(this)
            {
                UnconnectedMessagesEnabled = true,
                UpdateTime = 15,
                NatPunchEnabled = false,
                IPv6Enabled = false
            };

            _p2pConnectionHelper = new P2PConnectionHelper(_netClient);
            _p2pConnectionHelper.Connect();

            //_netClient.Start();
            //_netClient.Connect(PluginConfigSettings.Instance.CoopSettings.SITUDPHostIPV4, PluginConfigSettings.Instance.CoopSettings.SITUDPPort, "sit.core");

            Connect();
        }

        private async void Connect()
        {
            NatTraversalRequest natTraversalRequest = new NatTraversalRequest(_p2pConnectionHelper);
            var endPoint = await natTraversalRequest.NatPunchRequestAsync(MatchmakerAcceptPatches.GetGroupId(), MatchmakerAcceptPatches.Profile.ProfileId);

            if(endPoint != null) 
            {
                _netClient.Connect(endPoint, "sit.core");
            }
        }

        //private void OnAirdropLootPacketReceived(AirdropLootPacket packet, NetPeer peer)
        //{
        //    if (!Singleton<SITAirdropsManager>.Instantiated)
        //    {
        //        EFT.UI.ConsoleScreen.LogError("OnAirdropLootPacketReceived: Received loot package but manager is not instantiated!");
        //        return;
        //    }
        //    Singleton<SITAirdropsManager>.Instance.ReceiveBuildLootContainer(packet.Loot, packet.Config);
        //}

        //private void OnAirdropPacketReceived(AirdropPacket packet, NetPeer peer)
        //{
        //    if (Singleton<SITAirdropsManager>.Instantiated)
        //    {
        //        Singleton<SITAirdropsManager>.Instance.AirdropParameters = new()
        //        {
        //            Config = packet.Config,
        //            AirdropAvailable = packet.AirdropAvailable,
        //            PlaneSpawned = packet.PlaneSpawned,
        //            BoxSpawned = packet.BoxSpawned,
        //            DistanceTraveled = packet.DistanceTraveled,
        //            DistanceToTravel = packet.DistanceToTravel,
        //            DistanceToDrop = packet.DistanceToDrop,
        //            Timer = packet.Timer,
        //            DropHeight = packet.DropHeight,
        //            TimeToStart = packet.TimeToStart,
        //            RandomAirdropPoint = packet.BoxPoint,
        //            SpawnPoint = packet.SpawnPoint,
        //            LookPoint = packet.LookPoint
        //        };
        //    }
        //    else
        //    {
        //        EFT.UI.ConsoleScreen.LogError("OnAirdropPacketReceived: Received package but manager is not instantiated!");
        //    }
        //}

        //private void OnInformationPacketReceived(InformationPacket packet, NetPeer peer)
        //{
        //    if (!packet.IsRequest)
        //        ConnectedClients = packet.NumberOfPlayers;
        //}

        //private void OnAllCharacterRequestPacketReceived(AllCharacterRequestPacket packet, NetPeer peer)
        //{
        //    if (!packet.IsRequest)
        //    {
        //        EFT.UI.ConsoleScreen.Log($"Received CharacterRequest! ProfileID: {packet.PlayerInfo.Profile.ProfileId}, Nickname: {packet.PlayerInfo.Profile.Nickname}");
        //        if (packet.ProfileId != MyPlayer.ProfileId)
        //        {
        //            if (!CoopGameComponent.PlayersToSpawn.ContainsKey(packet.PlayerInfo.Profile.ProfileId))
        //                CoopGameComponent.PlayersToSpawn.TryAdd(packet.PlayerInfo.Profile.ProfileId, ESpawnState.None);
        //            if (!CoopGameComponent.PlayersToSpawnProfiles.ContainsKey(packet.PlayerInfo.Profile.ProfileId))
        //                CoopGameComponent.PlayersToSpawnProfiles.Add(packet.PlayerInfo.Profile.ProfileId, packet.PlayerInfo.Profile);

        //            CoopGameComponent.QueueProfile(packet.PlayerInfo.Profile, new Vector3(packet.Position.x, packet.Position.y + 0.5f, packet.Position.y), packet.IsAlive);
        //        }
        //    }
        //    else if (packet.IsRequest)
        //    {
        //        EFT.UI.ConsoleScreen.Log($"Received CharacterRequest from server, send my Profile.");
        //        AllCharacterRequestPacket requestPacket = new(MyPlayer.ProfileId)
        //        {
        //            IsRequest = false,
        //            PlayerInfo = new()
        //            {
        //                Profile = MyPlayer.Profile
        //            },
        //            IsAlive = MyPlayer.ActiveHealthController.IsAlive,
        //            Position = MyPlayer.Transform.position
        //        };
        //        _dataWriter.Reset();
        //        SendData(_dataWriter, ref requestPacket, DeliveryMethod.ReliableUnordered);
        //    }
        //}

        //private void OnCommonPlayerPacketReceived(CommonPlayerPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    var playerToApply = Players[packet.ProfileId];
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        playerToApply.CommonPlayerPackets.Enqueue(packet);
        //    }
        //}

        //private void OnInventoryPacketReceived(InventoryPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    var playerToApply = Players[packet.ProfileId];
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        playerToApply.InventoryPackets.Enqueue(packet);
        //    }
        //}

        //private void OnHealthPacketReceived(HealthPacket packet, NetPeer peer)
        //{
        //    EFT.UI.ConsoleScreen.Log($"{packet.ProfileId}");
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    var playerToApply = Players[packet.ProfileId];
        //    if (playerToApply != default && playerToApply != null)
        //    {
        //        playerToApply.HealthPackets.Enqueue(packet);
        //    }
        //}

        //private void OnFirearmControllerPacketReceived(WeaponPacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    var playerToApply = Players[packet.ProfileId];
        //    if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
        //    {
        //        playerToApply.FirearmPackets.Enqueue(packet);
        //    }
        //}

        //private void OnWeatherPacketReceived(WeatherPacket packet, NetPeer peer)
        //{
        //    var weatherController = EFT.Weather.WeatherController.Instance;
        //    if (weatherController != null)
        //    {
        //        var weatherDebug = weatherController.WeatherDebug;
        //        if (weatherDebug != null)
        //        {
        //            weatherDebug.Enabled = true;

        //            weatherDebug.CloudDensity = packet.CloudDensity;
        //            weatherDebug.Fog = packet.Fog;
        //            weatherDebug.LightningThunderProbability = packet.LightningThunderProbability;
        //            weatherDebug.Rain = packet.Rain;
        //            weatherDebug.Temperature = packet.Temperature;
        //            weatherDebug.TopWindDirection = new(packet.TopWindX, packet.TopWindY);

        //            Vector2 windDirection = new(packet.WindX, packet.WindY);

        //            // working dog sh*t, if you are the programmer, DON'T EVER DO THIS! - dounai2333
        //            static bool BothPositive(float f1, float f2) => f1 > 0 && f2 > 0;
        //            static bool BothNegative(float f1, float f2) => f1 < 0 && f2 < 0;
        //            static bool VectorIsSameQuadrant(Vector2 v1, Vector2 v2, out int flag)
        //            {
        //                flag = 0;
        //                if (v1.x != 0 && v1.y != 0 && v2.x != 0 && v2.y != 0)
        //                {
        //                    if ((BothPositive(v1.x, v2.x) && BothPositive(v1.y, v2.y))
        //                    || (BothNegative(v1.x, v2.x) && BothNegative(v1.y, v2.y))
        //                    || (BothPositive(v1.x, v2.x) && BothNegative(v1.y, v2.y))
        //                    || (BothNegative(v1.x, v2.x) && BothPositive(v1.y, v2.y)))
        //                    {
        //                        flag = 1;
        //                        return true;
        //                    }
        //                }
        //                else
        //                {
        //                    if (v1.x != 0 && v2.x != 0)
        //                    {
        //                        if (BothPositive(v1.x, v2.x) || BothNegative(v1.x, v2.x))
        //                        {
        //                            flag = 1;
        //                            return true;
        //                        }
        //                    }
        //                    else if (v1.y != 0 && v2.y != 0)
        //                    {
        //                        if (BothPositive(v1.y, v2.y) || BothNegative(v1.y, v2.y))
        //                        {
        //                            flag = 2;
        //                            return true;
        //                        }
        //                    }
        //                }
        //                return false;
        //            }

        //            for (int i = 1; i < WeatherClass.WindDirections.Count(); i++)
        //            {
        //                Vector2 direction = WeatherClass.WindDirections[i];
        //                if (VectorIsSameQuadrant(windDirection, direction, out int flag))
        //                {
        //                    weatherDebug.WindDirection = (EFT.Weather.WeatherDebug.Direction)i;
        //                    weatherDebug.WindMagnitude = flag switch
        //                    {
        //                        1 => windDirection.x / direction.x,
        //                        2 => windDirection.y / direction.y,
        //                        _ => weatherDebug.WindMagnitude
        //                    };
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            EFT.UI.ConsoleScreen.LogError("TimeAndWeather: WeatherDebug is null!");
        //        }
        //    }
        //    else
        //    {
        //        EFT.UI.ConsoleScreen.LogError("TimeAndWeather: WeatherController is null!");
        //    }
        //}

        //private void OnGameTimerPacketReceived(GameTimerPacket packet, NetPeer peer)
        //{
        //    CoopGameComponent coopGameComponent = CoopGameComponent.GetCoopGameComponent();
        //    if (coopGameComponent == null)
        //        return;

        //    if (MatchmakerAcceptPatches.IsClient)
        //    {
        //        var sessionTime = new TimeSpan(packet.Tick);

        //        if (coopGameComponent.LocalGameInstance is CoopGame coopGame)
        //        {
        //            var gameTimer = coopGame.GameTimer;
        //            if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
        //            {
        //                if (gameTimer.PastTime.TotalSeconds < 3)
        //                    return;

        //                var timeRemain = gameTimer.PastTime + sessionTime;

        //                if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
        //                    return;

        //                gameTimer.ChangeSessionTime(timeRemain);

        //                // FIXME: Giving SetTime() with empty exfil point arrays has a known bug that may cause client game crashes!
        //                coopGame.GameUi.TimerPanel.SetTime(gameTimer.StartDateTime.Value, coopGame.Profile_0.Info.Side, gameTimer.SessionSeconds(), []);
        //            }
        //        }
        //    }
        //}

        //private void OnPlayerStatePacketReceived(PlayerStatePacket packet, NetPeer peer)
        //{
        //    if (!Players.ContainsKey(packet.ProfileId))
        //        return;

        //    var playerToApply = Players[packet.ProfileId];
        //    if (playerToApply != default && playerToApply != null && !playerToApply.IsYourPlayer)
        //    {
        //        playerToApply.NewState = packet;
        //    }
        //}

        void Update()
        {
            _netClient.PollEvents();

            var peer = _netClient.FirstPeer;
            if (peer != null && peer.ConnectionState == ConnectionState.Connected)
            {
                //Basic lerp
                //_lerpTime += Time.deltaTime / Time.fixedDeltaTime;
            }
            else
            {
                _netClient.SendBroadcast([1], MatchmakerAcceptPatches.ServerPort);
            }
        }

        void OnDestroy()
        {
            if (_netClient != null)
                _netClient.Stop();
        }

        public void SendData<T>(NetDataWriter writer, ref T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            _netClient.FirstPeer.Send(writer, deliveryMethod);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            EFT.UI.ConsoleScreen.Log("[CLIENT] We connected to " + peer.EndPoint);
            NotificationManagerClass.DisplayMessageNotification($"Connected to server {peer.EndPoint}.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Friend);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            EFT.UI.ConsoleScreen.Log("[CLIENT] We received error " + socketErrorCode);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            _packetProcessor.ReadAllPackets(reader, peer);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
            {
                EFT.UI.ConsoleScreen.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
                _netClient.Connect(remoteEndPoint, "sit.core");
            }
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            Ping = latency;
        }

        public void OnConnectionRequest(LiteNetLib.ConnectionRequest request)
        {

        }

        public void OnPeerDisconnected(NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
        {
            EFT.UI.ConsoleScreen.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
        }

        public void SendDataToServer(byte[] data)
        {
            _netClient.FirstPeer.Send(data, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }
    }
}
