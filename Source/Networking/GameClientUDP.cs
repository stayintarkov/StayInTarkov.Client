using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using LiteNetLib;
using LiteNetLib.Utils;
using Sirenix.Utilities;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using STUN;

//using StayInTarkov.Coop.Players;
//using StayInTarkov.Networking.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityStandardAssets.Water;
using static BackendConfigManagerConfig;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking
{
    public class GameClientUDP : MonoBehaviour, INetEventListener, IGameClient
    {
        public Dictionary<string, IPEndPoint> ServerEndPoints = new Dictionary<string, IPEndPoint>();
        public NatHelper _natHelper;
        private LiteNetLib.NetManager _netClient;
        private NetDataWriter _dataWriter = new();
        private SITGameComponent CoopGameComponent { get; set; }
        public NetPacketProcessor _packetProcessor = new();
        public int ConnectedClients = 0;
        public ushort Ping { get; private set; } = 0;
        public float DownloadSpeedKbps { get; private set; } = 0;
        public float UploadSpeedKbps { get; private set; } = 0;
        public uint PacketLoss { get; private set; } = 0;
        private ManualLogSource Logger { get; set; }

        void Awake()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<SITGameComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GameClientUDP));

            //PublicEndPoint = new IPEndPoint(IPAddress.Parse(StayInTarkovPlugin.SITIPAddresses.ExternalAddresses.IPAddressV4), PluginConfigSettings.Instance.CoopSettings.SITUdpPort);
        }

        public async void Start()
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
                IPv6Enabled = false,
                PacketPoolSize = 999,
                EnableStatistics = true,
            };

            if(SITMatchmaking.IsClient)
            {
                var msg = $"Connecting to Nat Helper as {SITMatchmaking.Profile.ProfileId}...";
                EFT.UI.ConsoleScreen.Log(msg);
                Logger.LogDebug(msg);

                _natHelper = new NatHelper(_netClient, SITMatchmaking.Profile.ProfileId);
                _natHelper.Connect();

                if (!_natHelper.IsConnected())
                {
                    Logger.LogError("Could not connect to NatHelper");
                }

                msg = $"Getting Server Endpoints...";
                EFT.UI.ConsoleScreen.Log(msg);
                Logger.LogDebug(msg);

                ServerEndPoints = await _natHelper.GetEndpointsRequestAsync(SITMatchmaking.GetGroupId(), SITMatchmaking.Profile.ProfileId);

                msg = $"Found endpoints ${string.Join("\n", ServerEndPoints)}";
                EFT.UI.ConsoleScreen.Log(msg);
                Logger.LogDebug(msg);

                if (ServerEndPoints.ContainsKey("stun"))
                {
                    msg = $"Performing Nat Punch Request...";
                    EFT.UI.ConsoleScreen.Log(msg);
                    Logger.LogDebug(msg);

                    _natHelper.AddStunEndPoint();
                    await _natHelper.NatPunchRequestAsync(SITMatchmaking.GetGroupId(), SITMatchmaking.Profile.ProfileId, ServerEndPoints);
                    
                    if(_natHelper.PublicEndPoints.ContainsKey("stun"))
                        _netClient.Start(_natHelper.PublicEndPoints["stun"].Port);
                }

                if(!_netClient.IsRunning)
                    _netClient.Start();
;
                if (ServerEndPoints.ContainsKey("explicit"))
                {
                    var expli = ServerEndPoints["explicit"];
                    msg = $"Forcing connection to {expli}";
                    Logger.LogDebug(msg);
                    EFT.UI.ConsoleScreen.Log(msg);

                    _netClient.Connect(expli, "sit.core");
                }
                else
                {
                    // Broadcast for local connection
                    _netClient.SendBroadcast([1], SITMatchmaking.PublicPort);

                    var attemptedEndPoints = new List<IPEndPoint>();
                    foreach (var serverEndPoint in ServerEndPoints)
                    {
                        // Make sure we are not already connected
                        if (_netClient.ConnectedPeersCount > 0)
                            break;

                        // Make sure we only try proposed endpoints once
                        if (!attemptedEndPoints.Contains(serverEndPoint.Value))
                        {
                            msg = $"Connecting to {serverEndPoint.Value}";
                            Logger.LogDebug(msg);
                            EFT.UI.ConsoleScreen.Log(msg);

                            _netClient.Connect(serverEndPoint.Value, "sit.core");

                            attemptedEndPoints.Add(serverEndPoint.Value);
                        }
                    }
                }

                _natHelper.Close();
            }
            else if(SITMatchmaking.IsServer)
            {
                // Connect locally if we're the server.
                var endpoint = new IPEndPoint(IPAddress.Loopback, PluginConfigSettings.Instance.CoopSettings.UdpServerLocalPort);
                var msg = $"Server connecting as client to {endpoint}";
                Logger.LogDebug(msg);
                EFT.UI.ConsoleScreen.Log(msg);
                _netClient.Start();
                _netClient.Connect(endpoint, "sit.core");
            }
        }
        void Update()
        {
            _netClient.PollEvents();
        }

        public void ResetStats()
        {
            DownloadSpeedKbps = _netClient.Statistics.BytesReceived / 1024f;
            UploadSpeedKbps = _netClient.Statistics.BytesSent / 1024f;
            PacketLoss = (uint)(_netClient.Statistics.PacketLoss == 0 ? 0 : (100 * _netClient.Statistics.PacketLoss / _netClient.Statistics.PacketsSent));
            _netClient.Statistics.Reset();
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            var bytes = reader.GetRemainingBytes();
            SITGameServerClientDataProcessing.ProcessPacketBytes(bytes, Encoding.UTF8.GetString(bytes));

#if DEBUG
            if (_netClient.Statistics.PacketLossPercent > 0)
            {
                Logger.LogError($"Packet Loss {_netClient.Statistics.PacketLossPercent}%");
            }
#endif
        }

        void OnDestroy()
        {
            if (_netClient != null)
                _netClient.Stop();

            if (_natHelper != null)
                _natHelper.Close();
        }
        
        public void OnPeerConnected(NetPeer peer)
        {
            // Disconnect if more than one endpoint was reached
            if (_netClient.ConnectedPeersCount > 1)
            {
                peer.Disconnect();
                return;
            }

            EFT.UI.ConsoleScreen.Log("[CLIENT] We connected to " + peer.EndPoint);
            NotificationManagerClass.DisplayMessageNotification($"Connected to server {peer.EndPoint}.",
                EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Friend);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            EFT.UI.ConsoleScreen.Log("[CLIENT] We received error " + socketErrorCode);
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
            Ping = (ushort)latency;
        }

        public void OnConnectionRequest(LiteNetLib.ConnectionRequest request)
        {

        }

        public void OnPeerDisconnected(NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
        {
            EFT.UI.ConsoleScreen.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
        }

        int firstPeerErrorCount = 0;

        public void SendData(byte[] data)
        {
            if (_netClient == null)
            {
                EFT.UI.ConsoleScreen.LogError("[CLIENT] Could not communicate to the Server");
                return;
            }

            if (_netClient.FirstPeer == null)
            {
                string clientFirstPeerIsNullMessage = "[CLIENT] Could not communicate to the Server";
                EFT.UI.ConsoleScreen.LogError(clientFirstPeerIsNullMessage);
                Logger.LogError(clientFirstPeerIsNullMessage);

                firstPeerErrorCount++;

                if(firstPeerErrorCount == 30)
                {
                    Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("Connection Error"
                        , $"Connection Lost. Unable to communicate with Server. Error: {clientFirstPeerIsNullMessage}"
                        , ErrorScreen.EButtonType.OkButton
                        , 60
                        , () => { Singleton<ISITGame>.Instance.Stop(SITMatchmaking.Profile.ProfileId, ExitStatus.Survived, ""); }
                        , () => { Singleton<ISITGame>.Instance.Stop(SITMatchmaking.Profile.ProfileId, ExitStatus.Survived, ""); }
                        );

                    throw new Exception();
                }
                return;
            }

            _netClient.FirstPeer.Send(data, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }

        public void SendData<T>(ref T packet) where T : BasePacket
        {
            if (_netClient == null)
            {
                EFT.UI.ConsoleScreen.LogError("[CLIENT] Could not communicate to the Server");
                return;
            }

            if (_netClient.FirstPeer == null)
            {
                string clientFirstPeerIsNullMessage = "[CLIENT] Could not communicate to the Server";
                EFT.UI.ConsoleScreen.LogError(clientFirstPeerIsNullMessage);
                return;
            }

            using NetDataWriter writer = new NetDataWriter();
            _packetProcessor.WriteNetSerializable(writer, ref packet);
            if (_netClient.FirstPeer != null)
            {
                _netClient.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
