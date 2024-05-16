﻿using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Counters;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using Google.FlatBuffers;
using Newtonsoft.Json.Linq;
using StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Controllers.Health;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Coop.NetworkPacket.World;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Coop.Web;
using StayInTarkov.FlatBuffers;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using FBPacket = StayInTarkov.FlatBuffers.Packet;
using Rect = UnityEngine.Rect;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    /// <summary>
    /// Coop Game Component is the User 1-2-1 communication to the Server. This can be seen as an extension component to CoopGame.
    /// </summary>
    public class SITGameComponent : MonoBehaviour
    {
        #region Fields/Properties        
        public WorldInteractiveObject[] ListOfInteractiveObjects { get; set; }
        private AkiBackendCommunication RequestingObj { get; set; }
        public SITConfig SITConfig { get; private set; } = new SITConfig();
        public string ServerId { get; set; } = null;
        public long Timestamp { get; set; } = 0;
        public ushort AkiBackendPing = 0;

        /// <summary>
        /// ProfileId to Player instance
        /// </summary>
        public ConcurrentDictionary<string, CoopPlayer> Players { get; } = new();

        /// <summary>
        /// The Client Drones connected to this Raid
        /// </summary>
        public HashSet<CoopPlayerClient> PlayerClients { get; } = new();

        public IEnumerable<EFT.Player> PlayerUsers
        {
            get
            {
                if (Players == null)
                    yield return null;

                if (ProfileIdsUser == null)
                    yield return null;

                var keys = Players.Keys.Where(x => ProfileIdsUser.Contains(x)).ToArray();
                foreach (var key in keys)
                    yield return Players[key];


            }
        }

        public EFT.Player[] PlayerBots
        {
            get
            {
                if (LocalGameInstance is CoopSITGame coopGame)
                {
                    if (SITMatchmaking.IsClient || coopGame.Bots.Count == 0)
                        return Players
                            .Values
                            .Where(x => ProfileIdsAI.Contains(x.ProfileId) && x.ActiveHealthController.IsAlive)
                            .ToArray();

                    return coopGame.Bots.Values.ToArray();
                }

                return null;
            }
        }

        /// <summary>
        /// This is all the spawned players via the spawning process. Not anyone else.
        /// </summary>
        public Dictionary<string, EFT.Player> SpawnedPlayers { get; private set; } = new();

        public BepInEx.Logging.ManualLogSource Logger { get; private set; }
        public ConcurrentDictionary<string, ESpawnState> PlayersToSpawn { get; private set; } = new();
        public ConcurrentDictionary<string, PlayerInformationPacket> PlayersToSpawnPacket { get; private set; } = new();
        public Dictionary<string, Profile> PlayersToSpawnProfiles { get; private set; } = new();
        public ConcurrentDictionary<string, Vector3> PlayersToSpawnPositions { get; private set; } = new();
        public ConcurrentDictionary<string, string> PlayerInventoryMongoIds { get; private set; } = new();

        public HashSet<string> ProfileIdsAI { get; } = new();
        public HashSet<string> ProfileIdsUser { get; } = new();

        public List<LocalPlayer> SpawnedPlayersToFinalize { get; private set; } = new();

        public bool RunAsyncTasks { get; set; } = true;

        float screenScale = 1.0f;

        Camera GameCamera { get; set; }

        public ActionPacketHandlerComponent ActionPacketHandler { get; set; }

        public static SITGameComponent Instance { get; set; }

        #endregion

        #region Public Voids

        public static SITGameComponent GetCoopGameComponent()
        {
            if (Instance != null)
                return Instance;
#if DEBUG
            StayInTarkovHelperConstants.Logger.LogError($"Attempted to use {nameof(GetCoopGameComponent)} before {nameof(SITGameComponent)} has been created.");
            StayInTarkovHelperConstants.Logger.LogError(new System.Diagnostics.StackTrace());
#endif

            return null;
        }

        public static bool TryGetCoopGameComponent(out SITGameComponent coopGameComponent)
        {
            coopGameComponent = GetCoopGameComponent();
            return coopGameComponent != null;
        }

        public static string GetServerId()
        {
            return SITMatchmaking.GetGroupId();
        }
        #endregion

        #region Unity Component Methods

        /// <summary>
        /// Unity Component Awake Method
        /// </summary>
        void Awake()
        {
            // ----------------------------------------------------
            // Create a BepInEx Logger for CoopGameComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITGameComponent));
            Logger.LogDebug($"{nameof(SITGameComponent)}:{nameof(Awake)}");

            ActionPacketHandler = gameObject.GetOrAddComponent<ActionPacketHandlerComponent>();
            gameObject.AddComponent<SITGameGUIComponent>();

            SITCheck();
        }

        /// <summary>
        /// Check the StayInTarkov assembly hasn't been messed with.
        /// </summary>
        void SITCheck()
        {
            // Check the StayInTarkov assembly hasn't been messed with.
            SITCheckConfirmed[0] = StayInTarkovHelperConstants
                .SITTypes
                .Any(x => x.Name ==
                Encoding.UTF8.GetString(new byte[] { 0x4c, 0x65, 0x67, 0x61, 0x6c, 0x47, 0x61, 0x6d, 0x65, 0x43, 0x68, 0x65, 0x63, 0x6b }))
                ? (byte)0x1 : (byte)0x0;
        }



        /// <summary>
        /// Unity Component Start Method
        /// </summary>
        async void Start()
        {
            Instance = this;

            // Instantiate the Requesting Object for Aki Communication
            RequestingObj = AkiBackendCommunication.GetRequestInstance(false, Logger);

            // Request SIT Config
            await RequestingObj.PostJsonAsync<SITConfig>("/SIT/Config", "{}").ContinueWith(x =>
            {
                if (x.IsCanceled || x.IsFaulted)
                {
                    SITConfig = new SITConfig();
                    Logger.LogError("SIT Config Failed!");
                }
                else
                {
                    SITConfig = x.Result;
                    Logger.LogDebug("SIT Config received Successfully!");
                    Logger.LogDebug(SITConfig.ToJson());

                }
            });

            // Run any methods you wish every second
            StartCoroutine(EverySecondCoroutine());

            // Enable the Coop Patches
            CoopPatches.EnableDisablePatches();

            Singleton<GameWorld>.Instance.AfterGameStarted += GameWorld_AfterGameStarted; ;

            // In game ping system.
            if (Singleton<FrameMeasurer>.Instantiated)
            {
                var rttMs = 2 * (Singleton<IGameClient>.Instance?.Ping ?? 1);
                FrameMeasurer instance = Singleton<FrameMeasurer>.Instance;
                instance.PlayerRTT = rttMs;
                instance.ServerFixedUpdateTime = rttMs;
                instance.ServerTime = rttMs;
                instance.NetworkQuality.CreateMeasurers();
            }
        }

        private IEnumerator SendPlayerStatePacket()
        {
            var players = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            foreach (var player in players)
            {
                if (player == null)
                    continue;

                if (player is CoopPlayerClient)
                    continue;

                if (!player.enabled)
                    continue;

                if (!player.isActiveAndEnabled)
                    continue;

                var builder = new FlatBufferBuilder(1024);

                var profileId = builder.CreateString(player.ProfileId);

                PlayerState.StartPlayerState(builder);

                // Iterate over the BodyParts
                {
                    // no Span<T>, Sadge
                    var currents = new float[Enum.GetValues(typeof(EBodyPart)).Length];
                    var maximums = new float[Enum.GetValues(typeof(EBodyPart)).Length];
                    foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                    {
                        var health = player.HealthController.GetBodyPartHealth(bodyPart);
                        currents[(byte)bodyPart] = health.Current;
                        maximums[(byte)bodyPart] = health.Maximum;
                    }
                    PlayerState.AddBodyPartsHealth(builder, BodyPartsHealth.CreateBodyPartsHealth(builder, currents, maximums));
                }

                // TODO(belette) add this later? seems unused today since these packets only carry profileId and no relevant info atm
                //if (player.HealthController is SITHealthController sitHealthController)
                //{
                //    var tmpHealthEffectPacketList = new List<PlayerHealthEffectPacket>();
                //    while (sitHealthController.PlayerHealthEffectPackets.TryDequeue(out var p))
                //    {
                //        tmpHealthEffectPacketList.Add(p);
                //    }
                //    playerHealth.HealthEffectPackets = tmpHealthEffectPacketList.ToArray();
                //}

                PlayerState.AddProfileId(builder, profileId);
                PlayerState.AddIsAlive(builder, player.HealthController.IsAlive);
                PlayerState.AddEnergy(builder, player.HealthController.Energy.Current);
                PlayerState.AddHydration(builder, player.HealthController.Hydration.Current);
                PlayerState.AddPosition(builder, Vec3.CreateVec3(builder, player.Position.x, player.Position.y, player.Position.z));
                PlayerState.AddRotation(builder, Vec2.CreateVec2(builder, player.Rotation.x, player.Rotation.y));
                PlayerState.AddHeadRotation(builder, Vec3.CreateVec3(builder, player.HeadRotation.x, player.HeadRotation.y, player.HeadRotation.z));
                PlayerState.AddMovementDirection(builder, Vec2.CreateVec2(builder, player.MovementContext.MovementDirection.x, player.MovementContext.MovementDirection.y));
                PlayerState.AddState(builder, (FlatBuffers.EPlayerState)player.MovementContext.CurrentState.Name);
                PlayerState.AddTilt(builder, player.MovementContext.Tilt);
                PlayerState.AddStep(builder, (sbyte)player.MovementContext.Step);
                PlayerState.AddAnimatorStateIndex(builder, (byte)player.MovementContext.CurrentAnimatorStateIndex);
                PlayerState.AddCharacterMovementSpeed(builder, player.MovementContext.CharacterMovementSpeed);
                PlayerState.AddIsProne(builder, player.MovementContext.IsInPronePose);
                PlayerState.AddPoseLevel(builder, player.MovementContext.PoseLevel);
                PlayerState.AddIsSprinting(builder, player.MovementContext.IsSprintEnabled);
                PlayerState.AddInputDirection(builder, Vec2.CreateVec2(builder, player.InputDirection.x, player.InputDirection.y));
                PlayerState.AddLeftStance(builder, player.MovementContext.LeftStanceController.LastAnimValue);
                PlayerState.AddHandsExhausted(builder, player.Physical.SerializationStruct.HandsExhausted);
                PlayerState.AddStaminaExhausted(builder, player.Physical.SerializationStruct.StaminaExhausted);
                PlayerState.AddOxygenExhausted(builder, player.Physical.SerializationStruct.OxygenExhausted);
                PlayerState.AddBlindfire(builder, (sbyte)player.MovementContext.BlindFire);
                PlayerState.AddLinearSpeed(builder, player.MovementContext.ActualLinearVelocity);
                var pstateOffset = PlayerState.EndPlayerState(builder);

                FBPacket.StartPacket(builder);
                FBPacket.AddPacketType(builder, AnyPacket.player_state);
                FBPacket.AddPacket(builder, pstateOffset.Value);
                var packetOffset = FBPacket.EndPacket(builder);

                builder.Finish(packetOffset.Value);

                if (Singleton<ISITGame>.Instance.GameClient is GameClientUDP udp)
                {
                    var seg = builder.DataBuffer.ToArraySegment(builder.DataBuffer.Position, builder.DataBuffer.Length - builder.DataBuffer.Position);
                    udp.SendData(seg.Array, seg.Offset, seg.Count, SITGameServerClientDataProcessing.FLATBUFFER_CHANNEL_NUM, LiteNetLib.DeliveryMethod.Sequenced);
                }
                else if (Singleton<ISITGame>.Instance.GameClient is GameClientTCPRelay)
                {
                    GameClient.SendData(builder.SizedByteArray());
                }
            }

            // ----------------------------------------------------------------------------------------------------
            // Paulov: Keeping this here as a note. DO NOT DELETE.
            // Testing the length MTU. If over the traffic limit, then try sending lots of smaller packets 
            // After testing. This was a bad idea. It caused multiple 1% packet loss over Udp instead of very minor packet loss when sending large packets
            // The 1% packet loss resulted in bots looking the wrong direction and all kinds of weird behavior. Not great.
            //            if (serialized.Length >= 1460)
            //            {
            //#if DEBUG
            //                Logger.LogError($"{nameof(playerStatesPacket)} is {serialized.Length} in Length, this will be network split");
            //#endif
            //                foreach(var psp in playerStatesPacket.PlayerStates)
            //                {
            //                    // wait a little bit for the previous packet to process thru
            //                    yield return new WaitForSeconds(0.033f);
            //                    GameClient.SendData(serialized);
            //                }
            //            }
            // ----------------------------------------------------------------------------------------------------

            yield return new WaitForSeconds(PluginConfigSettings.Instance.CoopSettings.SETTING_PlayerStateTickRateInMS / 1000f);
            StartCoroutine(SendPlayerStatePacket());
        }

        private void GameWorld_AfterGameStarted()
        {
            GameWorldGameStarted = true;
            Logger.LogDebug(nameof(GameWorld_AfterGameStarted));
            //if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any())
            //{
            //    // Send My Player to Aki, so that other clients know about me
            //    CoopSITGame.SendPlayerDataToServer((LocalPlayer)Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer));
            //}

            this.GetOrAddComponent<SITGameGCComponent>();
            this.GetOrAddComponent<SITGameTimeAndWeatherSyncComponent>();
            this.GetOrAddComponent<SITGameExtractionComponent>();

            StartCoroutine(SendPlayerStatePacket());

            // tell clients which exfils are disabled
            if (SITMatchmaking.IsServer)
            {
                foreach (var point in ExfiltrationControllerClass.Instance.ExfiltrationPoints)
                {
                    if (point.Status == EExfiltrationStatus.NotPresent)
                    {
                        UpdateExfiltrationPointPacket packet = new()
                        {
                            PointName = point.Settings.Name,
                            Command = point.Status,
                            QueuedPlayers = point.QueuedPlayers
                        };
                        GameClient.SendData(packet.Serialize());
                    }
                }
            }

            // Get a List of Interactive Objects (this is a slow method), so run once here to maintain a reference
            ListOfInteractiveObjects = FindObjectsOfType<WorldInteractiveObject>();
        }



        /// <summary>
        /// This is a simple coroutine to allow methods to run every second.
        /// </summary>
        /// <returns></returns>
        private IEnumerator EverySecondCoroutine()
        {
            var waitSeconds = new WaitForSeconds(1.0f);

            while (RunAsyncTasks)
            {
                yield return waitSeconds;

                try
                {
                    if (!Singleton<ISITGame>.Instantiated)
                        continue;

                    if (!Singleton<GameWorld>.Instantiated)
                        continue;

                    if (!GameWorldGameStarted)
                        continue;

                    //Logger.LogDebug($"DEBUG: {nameof(EverySecondCoroutine)}");

                    var coopGame = Singleton<ISITGame>.Instance;

                    var world = Singleton<GameWorld>.Instance;

                    // Add players who have joined to the AI Enemy Lists
                    var botController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BaseLocalGame<EftGamePlayerOwner>), typeof(BotsController)).GetValue(Singleton<ISITGame>.Instance);
                    if (botController != null)
                    {
                        while (PlayersForAIToTarget.TryDequeue(out var otherPlayer))
                        {
                            Logger.LogDebug($"Adding {otherPlayer.Profile.Nickname} to Enemy list");
                            botController.AddActivePLayer(otherPlayer);
                            botController.AddEnemyToAllGroups(otherPlayer, otherPlayer, otherPlayer);
                        }
                    }

                    if (Singleton<ISITGame>.Instance.GameClient is GameClientUDP udp)
                    {
                        coopGame.GameClient.ResetStats();
                    }

                    ProcessOtherModsSpawnedPlayers();

                }
                catch (Exception ex)
                {
                    Logger.LogError($"{nameof(EverySecondCoroutine)}: caught exception:\n{ex}");
                }
            }
        }

        private void ProcessOtherModsSpawnedPlayers()
        {
            // If another mod has spawned people, attempt to handle it.
            foreach (var p in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if (!Players.ContainsKey(p.ProfileId))
                {
                    // As these created players are unlikely to be CoopPlayer, throw an error!
                    if ((p as CoopPlayer) == null)
                    {
                        Logger.LogError($"Player of Id:{p.ProfileId} is not found in the SIT {nameof(Players)} list?!");
                    }
                }
            }
        }

        void OnDestroy()
        {
            StayInTarkovHelperConstants.Logger.LogDebug($"CoopGameComponent:OnDestroy");

            Players.Clear();
            PlayersToSpawnProfiles.Clear();
            PlayersToSpawnPositions.Clear();
            PlayersToSpawnPacket.Clear();
            RunAsyncTasks = false;
            StopCoroutine(EverySecondCoroutine());

            CoopPatches.EnableDisablePatches();
            GameObject.Destroy(this.GetComponent<SITGameGCComponent>());
            GameObject.Destroy(this.GetComponent<SITGameTimeAndWeatherSyncComponent>());
            Instance = null;
        }

        public bool RequestQuitGame { get; set; }
        int ForceQuitGamePressed = 0;

        /// <summary>
        /// The state your character or game is in to Quit.
        /// </summary>
        public enum EQuitState
        {
            NONE = -1,
            YouAreDead,
            YouAreDeadAsHost,
            YouAreDeadAsClient,
            YourTeamIsDead,
            YourTeamHasExtracted,
            YouHaveExtractedOnlyAsHost,
            YouHaveExtractedOnlyAsClient
        }

        public EQuitState GetQuitState()
        {
            var quitState = EQuitState.NONE;

            if (!Singleton<ISITGame>.Instantiated)
                return quitState;

            var coopGame = Singleton<ISITGame>.Instance;
            if (coopGame == null)
                return quitState;

            if (Players == null)
                return quitState;

            if (PlayerUsers == null)
                return quitState;

            if (PlayerUsers.Count() == 0)
                return quitState;

            if (coopGame.ExtractedPlayers == null)
                return quitState;

            var numberOfPlayersDead = PlayerUsers.Count(x => !x.HealthController.IsAlive);
            var numberOfPlayersAlive = PlayerUsers.Count(x => x.HealthController.IsAlive);
            var numberOfPlayersExtracted = coopGame.ExtractedPlayers.Count;

            var world = Singleton<GameWorld>.Instance;

            // You are playing with a team
            if (PlayerUsers.Count() > 1)
            {
                // All Player's in the Raid are dead
                if (PlayerUsers.Count() == numberOfPlayersDead)
                {
                    quitState = EQuitState.YourTeamIsDead;
                }
                else if (!world.MainPlayer.HealthController.IsAlive)
                {
                    if (SITMatchmaking.IsClient)
                        quitState = EQuitState.YouAreDeadAsClient;
                    else if (SITMatchmaking.IsServer)
                        quitState = EQuitState.YouAreDeadAsHost;
                }
            }
            else if (PlayerUsers.Any(x => !x.HealthController.IsAlive))
            {
                quitState = EQuitState.YouAreDead;
            }

            // -------------------------
            // Extractions
            if (coopGame.ExtractedPlayers.Contains(world.MainPlayer.ProfileId))
            {
                if (SITMatchmaking.IsClient)
                    quitState = EQuitState.YouHaveExtractedOnlyAsClient;
                else if (SITMatchmaking.IsServer)
                    quitState = EQuitState.YouHaveExtractedOnlyAsHost;
            }

            // If there are any players alive. Number of players Alive Equals Numbers of players Extracted
            // OR Number of Players Equals Number of players Extracted 
            if (numberOfPlayersAlive > 0 && numberOfPlayersAlive == numberOfPlayersExtracted || PlayerUsers.Count() == numberOfPlayersExtracted)
            {
                quitState = EQuitState.YourTeamHasExtracted;
            }
            return quitState;
        }

        /// <summary>
        /// This handles the ways of exiting the active game session
        /// </summary>
        void ProcessQuitting()
        {
            var quitState = GetQuitState();

            if (
                PluginConfigSettings.Instance.CoopSettings.SETTING_PressToExtractKey.Value.IsDown()
                &&
                quitState != EQuitState.NONE
                && !RequestQuitGame
                )
            {
                RequestQuitGame = true;

                // If you are the server / host
                if (SITMatchmaking.IsServer)
                {
                    // A host needs to wait for the team to extract or die!
                    if (PlayerUsers.Count() > 1 && (quitState == EQuitState.YouAreDeadAsHost || quitState == EQuitState.YouHaveExtractedOnlyAsHost))
                    {
                        NotificationManagerClass.DisplayWarningNotification("HOSTING: You cannot exit the game until all clients have escaped or dead");
                        RequestQuitGame = false;
                        return;
                    }
                    else
                    {
                        FindALocationAndProcessQuit();
                    }
                }
                else
                {
                    FindALocationAndProcessQuit();
                }
                return;
            }
            else if (
                PluginConfigSettings.Instance.CoopSettings.SETTING_PressToForceExtractKey.Value.IsDown()
                &&
                quitState != EQuitState.NONE
                && !RequestQuitGame
                )
            {
                RequestQuitGame = true;

                // If you are the server / host
                if (SITMatchmaking.IsServer)
                {
                    // A host needs to wait for the team to extract or die!
                    if (ForceQuitGamePressed < 1)
                    {
                        NotificationManagerClass.DisplayWarningNotification("HOSTING: Press again to force stop the game!");
                        ForceQuitGamePressed += 1;
                        RequestQuitGame = false;
                        return;
                    }
                    else
                    {
                        if (quitState == EQuitState.YouAreDeadAsHost || quitState == EQuitState.YouHaveExtractedOnlyAsHost || quitState == EQuitState.YourTeamHasExtracted || quitState == EQuitState.YourTeamIsDead)
                        {
                            ForceQuitGamePressed = 0;
                            FindALocationAndProcessQuit();
                        }
                    }
                }
                return;
            }
        }

        private void FindALocationAndProcessQuit()
        {
            Logger.LogDebug($"{nameof(FindALocationAndProcessQuit)}");
            Logger.LogDebug($"{nameof(FindALocationAndProcessQuit)}:{nameof(Singleton<ISITGame>.Instance.MyExitStatus)}:{Singleton<ISITGame>.Instance.MyExitStatus}");

            var quitState = GetQuitState();
            Logger.LogDebug($"{nameof(FindALocationAndProcessQuit)}:{nameof(quitState)}:{quitState}");

            // OnDied in GameMode should set this to ExitStatus.Killed
            // Only override if we are the default MissingInAction
            if (Singleton<ISITGame>.Instance.MyExitStatus == ExitStatus.MissingInAction)
            {
                switch (quitState)
                {
                    case EQuitState.YourTeamIsDead:
                    case EQuitState.YouAreDead:
                    case EQuitState.YouAreDeadAsHost:
                    case EQuitState.YouAreDeadAsClient:
                        Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Killed;
                        break;
                    case EQuitState.YouHaveExtractedOnlyAsClient:
                    case EQuitState.YouHaveExtractedOnlyAsHost:
                        Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Survived;
                        break;
                }

                if (PlayerUsers.Count() == 1 && quitState == EQuitState.YourTeamHasExtracted)
                    Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Survived;

                if (Singleton<ISITGame>.Instance.ExtractedPlayers.Contains(Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
                    Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Survived;

                if (!Singleton<GameWorld>.Instance.MainPlayer.HealthController.IsAlive)
                    Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Killed;
            }
            else if (Singleton<ISITGame>.Instance.MyExitStatus == ExitStatus.Runner)
            {
                Singleton<ISITGame>.Instance.MyExitStatus = ExitStatus.Runner;
            }


            if (Singleton<ISITGame>.Instance.MyExitLocation == null)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                List<ScavExfiltrationPoint> scavExfilFiltered = new();
                List<ExfiltrationPoint> pmcExfilPiltered = new();
                foreach (var exfil in gameWorld.ExfiltrationController.ExfiltrationPoints)
                {
                    if (exfil is ScavExfiltrationPoint scavExfil)
                    {
                        scavExfilFiltered.Add(scavExfil);
                    }
                    else
                    {
                        pmcExfilPiltered.Add(exfil);
                    }
                }

                var playerPos = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                var minDist = Mathf.Infinity;
                var closestExitLocation = "";
                if (RaidChangesUtil.IsScavRaid)
                {
                    foreach (var filter in scavExfilFiltered)
                    {
                        var dist = Vector3.Distance(filter.gameObject.transform.position, playerPos);
                        if (dist < minDist)
                        {
                            closestExitLocation = filter.Settings.Name;
                            minDist = dist;
                        }
                    }
                }
                else
                {
                    foreach (var filter in pmcExfilPiltered)
                    {
                        var dist = Vector3.Distance(filter.gameObject.transform.position, playerPos);
                        if (dist < minDist)
                        {
                            closestExitLocation = filter.Settings.Name;
                            minDist = dist;
                        }
                    }
                }
#if DEBUG
                Logger.LogDebug($"{nameof(ProcessQuitting)}: Original MyExitLocation is null::POS::3");
                Logger.LogDebug($"{nameof(ProcessQuitting)}: Using {closestExitLocation} as MyExitLocation");
#endif
                Singleton<ISITGame>.Instance.Stop(
                    Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                    , Singleton<ISITGame>.Instance.MyExitStatus
                    , closestExitLocation
                    , 0);
            }
            else
            {
                Singleton<ISITGame>.Instance.Stop(
                    Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                    , Singleton<ISITGame>.Instance.MyExitStatus
                    , Singleton<ISITGame>.Instance.MyExitLocation
                    , 0);
            }
        }

        /// <summary>
        /// This handles the possibility the server has stopped / disconnected and exits your player out of the game
        /// </summary>
        void ProcessServerHasStopped()
        {
            if (ServerHasStopped && !ServerHasStoppedActioned)
            {
                ServerHasStoppedActioned = true;
                try
                {
                    if (Singleton<ISITGame>.Instance.MyExitLocation == null)
                    {
                        var gameWorld = Singleton<GameWorld>.Instance;
                        List<ScavExfiltrationPoint> scavExfilFiltered = new();
                        List<ExfiltrationPoint> pmcExfilPiltered = new();
                        foreach (var exfil in gameWorld.ExfiltrationController.ExfiltrationPoints)
                        {
                            if (exfil is ScavExfiltrationPoint scavExfil)
                            {
                                scavExfilFiltered.Add(scavExfil);
                            }
                            else
                            {
                                pmcExfilPiltered.Add(exfil);
                            }
                        }

                        var playerPos = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                        var minDist = Mathf.Infinity;
                        var closestExitLocation = "";
                        if (RaidChangesUtil.IsScavRaid)
                        {
                            foreach (var filter in scavExfilFiltered)
                            {
                                var dist = Vector3.Distance(filter.gameObject.transform.position, playerPos);
                                if (dist < minDist)
                                {
                                    closestExitLocation = filter.Settings.Name;
                                    minDist = dist;
                                }
                            }
                        }
                        else
                        {
                            foreach (var filter in pmcExfilPiltered)
                            {
                                var dist = Vector3.Distance(filter.gameObject.transform.position, playerPos);
                                if (dist < minDist)
                                {
                                    closestExitLocation = filter.Settings.Name;
                                    minDist = dist;
                                }
                            }
                        }
#if DEBUG
                        Logger.LogDebug($"{nameof(ProcessQuitting)}: Original MyExitLocation is null::POS::4");
                        Logger.LogDebug($"{nameof(ProcessQuitting)}: Using {closestExitLocation} as MyExitLocation");
#endif
                        Singleton<ISITGame>.Instance.Stop(
                            Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                            , Singleton<ISITGame>.Instance.MyExitStatus
                            , closestExitLocation
                            , 0);
                    }
                    else
                    {
                        Singleton<ISITGame>.Instance.Stop(
                            Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                            , Singleton<ISITGame>.Instance.MyExitStatus
                            , Singleton<ISITGame>.Instance.MyExitLocation
                            , 0);
                    }
                }
                catch { }
                return;
            }
        }

        void Update()
        {
            GameCamera = Camera.current;

            if (!Singleton<ISITGame>.Instantiated)
                return;

            ProcessQuitting();
            ProcessServerHasStopped();
            ProcessServerCharacters();

            if (Players == null)
                return;

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (RequestingObj == null)
                return;

            if (SpawnedPlayersToFinalize == null)
                return;

            List<LocalPlayer> SpawnedPlayersToRemoveFromFinalizer = new();
            foreach (var p in SpawnedPlayersToFinalize)
            {
                SetWeaponInHandsOfNewPlayer(p, () =>
                {

                    SpawnedPlayersToRemoveFromFinalizer.Add(p);
                });
            }
            foreach (var p in SpawnedPlayersToRemoveFromFinalizer)
            {
                SpawnedPlayersToFinalize.Remove(p);
            }

            // In game ping system.
            if (Singleton<FrameMeasurer>.Instantiated)
            {
                var rttMs = 2 * (Singleton<IGameClient>.Instance?.Ping ?? 1);
                FrameMeasurer instance = Singleton<FrameMeasurer>.Instance;
                instance.PlayerRTT = rttMs;
                instance.ServerFixedUpdateTime = rttMs;
                instance.ServerTime = rttMs;
                //instance.NetworkQuality.CreateMeasurers();
            }

            if (Singleton<PreloaderUI>.Instantiated && SITCheckConfirmed[0] == 0 && SITCheckConfirmed[1] == 0)
            {
                SITCheckConfirmed[1] = 1;
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("", StayInTarkovPlugin.IllegalMessage, ErrorScreen.EButtonType.QuitButton, 60, () => { Application.Quit(); }, () => { Application.Quit(); });
            }
        }

        byte[] SITCheckConfirmed { get; } = new byte[2] { 0, 0 };

        #endregion

        //private IEnumerator ProcessServerCharacters()
        private void ProcessServerCharacters()
        {
            //var waitEndOfFrame = new WaitForEndOfFrame();

            //if (GetServerId() == null)
            //    yield return waitEndOfFrame;

            //var waitSeconds = new WaitForSeconds(0.5f);

            //while (RunAsyncTasks)
            //{
            //    yield return waitSeconds;
            foreach (var p in PlayersToSpawn)
            {
                // If not showing drones. Check whether the "Player" has been registered, if they have, then ignore the drone
                if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
                {
                    if (Singleton<GameWorld>.Instance.AllAlivePlayersList.Any(x => x.ProfileId == p.Key))
                    {
                        if (PlayersToSpawn.ContainsKey(p.Key))
                            PlayersToSpawn[p.Key] = ESpawnState.Ignore;

                        continue;
                    }

                    if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.ProfileId == p.Key))
                    {
                        if (PlayersToSpawn.ContainsKey(p.Key))
                            PlayersToSpawn[p.Key] = ESpawnState.Ignore;

                        continue;
                    }

                    if (Players.Any(x => x.Key == p.Key))
                    {
                        if (PlayersToSpawn.ContainsKey(p.Key))
                            PlayersToSpawn[p.Key] = ESpawnState.Ignore;

                        continue;
                    }
                }


                if (PlayersToSpawn[p.Key] == ESpawnState.Ignore)
                    continue;

                if (PlayersToSpawn[p.Key] == ESpawnState.Spawned)
                    continue;

                Vector3 newPosition = PlayersToSpawnPacket[p.Key].BodyPosition;
                //ProcessPlayerBotSpawn(PlayersToSpawnPacket[p.Key], p.Key, newPosition, false, PlayersToSpawnPacket[p.Key].InitialInventoryMongoId);
                ProcessPlayerBotSpawn(PlayersToSpawnPacket[p.Key], p.Key, newPosition, PlayersToSpawnPacket[p.Key].IsAI, PlayersToSpawnPacket[p.Key].InitialInventoryMongoId);
            }


            //    yield return waitEndOfFrame;
            //}
        }

        private void ProcessPlayerBotSpawn(PlayerInformationPacket packet, string profileId, Vector3 newPosition, bool isBot, string mongoId)
        {
            // If not showing drones. Check whether the "Player" has been registered, if they have, then ignore the drone
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
            {
                if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.ProfileId == profileId))
                {
                    if (PlayersToSpawn.ContainsKey(profileId))
                        PlayersToSpawn[profileId] = ESpawnState.Ignore;

                    return;
                }

                if (Players.Keys.Any(x => x == profileId))
                {
                    if (PlayersToSpawn.ContainsKey(profileId))
                        PlayersToSpawn[profileId] = ESpawnState.Ignore;

                    return;
                }
            }

            if (!PlayerInventoryMongoIds.ContainsKey(profileId))
                PlayerInventoryMongoIds.TryAdd(profileId, mongoId);


            if (!PlayersToSpawnPositions.ContainsKey(profileId))
                PlayersToSpawnPositions.TryAdd(profileId, newPosition);

            if (!PlayersToSpawnProfiles.ContainsKey(profileId))
            {
                PlayersToSpawnProfiles.Add(profileId, null);

                Logger.LogDebug($"ProcessPlayerBotSpawn:{profileId}");
                //if (packet.Profile == null)
                //    return;

                Profile profile = new();
                profile = packet.Profile;
                //profile.AccountId = packet.AccountId;
                //profile.AchievementsData = new Dictionary<string, int>();
                //profile.BonusController = new BonusController();
                //profile.Bonuses = null;
                //profile.CheckedChambers = new();
                //profile.CheckedMagazines = new();
                //profile.Customization = new Customization(packet.Customization);
                //profile.EftStats = new();
                //profile.Encyclopedia = new();
                //profile.Id = packet.ProfileId;
                //profile.Info = new ProfileInfo();
                //profile.Info.Nickname = packet.NickName;
                //profile.Info.Side = packet.Side;
                //profile.Info.Voice = packet.Voice;
                //profile.Inventory = packet.Inventory;
                //profile.Stats = new PmcStats();
                //profile.Health = packet.ProfileHealth;
                profile.Skills.StartClientMode();
                PlayersToSpawnProfiles[profileId] = profile;
            }
            else
                CreatePhysicalOtherPlayerOrBot(PlayersToSpawnProfiles[profileId], false);

            //if (packet.ContainsKey("profileJson"))
            //{
            //    if (packet["profileJson"].ToString().TrySITParseJson(out profile))
            //    {
            //        //Logger.LogInfo("Obtained Profile");
            //        profile.Skills.StartClientMode();
            //        // Send to be loaded
            //        PlayersToSpawnProfiles[profileId] = profile;
            //    }
            //    else
            //    {
            //        Logger.LogError("Unable to Parse Profile");
            //        PlayersToSpawn[profileId] = ESpawnState.Error;
            //        return;
            //    }
            //}
        }

        private void CreatePhysicalOtherPlayerOrBot(Profile profile, bool isDead = false)
        {
            try
            {
                // A final check to stop duplicate clones spawning on Server
                if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
                {
                    if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.ProfileId == profile.ProfileId))
                        return;

                    if (Singleton<GameWorld>.Instance.AllAlivePlayersList.Any(x => x.ProfileId == profile.ProfileId))
                        return;

                    if (Players.Keys.Any(x => x == profile.ProfileId))
                        return;
                }

                if (Players == null)
                {
                    Logger.LogError("Players is NULL!");
                    return;
                }

                int playerId = Players.Count + Singleton<GameWorld>.Instance.RegisteredPlayers.Count + 1;
                if (profile == null)
                {
                    Logger.LogError("CreatePhysicalOtherPlayerOrBot Profile is NULL!");
                    return;
                }

                PlayersToSpawn.TryAdd(profile.ProfileId, ESpawnState.None);
                if (PlayersToSpawn[profile.ProfileId] == ESpawnState.None)
                {
                    PlayersToSpawn[profile.ProfileId] = ESpawnState.Loading;
                    IEnumerable<ResourceKey> allPrefabPaths = profile.GetAllPrefabPaths();
                    if (allPrefabPaths.Count() == 0)
                    {
                        Logger.LogError($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::PrefabPaths are empty!");
                        PlayersToSpawn[profile.ProfileId] = ESpawnState.Error;
                        return;
                    }

                    Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, allPrefabPaths.ToArray(), JobPriority.General)
                        .ContinueWith(x =>
                        {
                            if (x.IsCompleted)
                            {
                                PlayersToSpawn[profile.ProfileId] = ESpawnState.Spawning;
                                Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Load Complete.");
                            }
                            else if (x.IsFaulted)
                            {
                                Logger.LogError($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Load Failed.");
                            }
                            else if (x.IsCanceled)
                            {
                                Logger.LogError($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Load Cancelled?.");
                            }
                        })
                        ;

                    return;
                }

                // ------------------------------------------------------------------
                // Its loading on the previous pass, ignore this one until its finished
                if (PlayersToSpawn[profile.ProfileId] == ESpawnState.Loading)
                {
                    //Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Is still loading");
                    return;
                }

                // ------------------------------------------------------------------
                // It has already spawned, we should never reach this point if Players check is working in previous step
                if (PlayersToSpawn[profile.ProfileId] == ESpawnState.Spawned)
                {
                    Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Is already spawned");
                    return;
                }

                // Move this here. Ensure that this is run before it attempts again on slow PCs
                PlayersToSpawn[profile.ProfileId] = ESpawnState.Spawned;

                // ------------------------------------------------------------------
                // Create Local Player drone
                var otherPlayer = SpawnCharacter(profile, PlayersToSpawnPositions[profile.Id], playerId);
                if (otherPlayer == null)
                {
                    PlayersToSpawn[profile.ProfileId] = ESpawnState.Spawning;
                    return;
                }

                if (isDead)
                {
                    // Logger.LogDebug($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: CreatePhysicalOtherPlayerOrBot::Killing localPlayer with ID {playerId}");
                    otherPlayer.ActiveHealthController.Kill(EDamageType.Undefined);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

        }

        private LocalPlayer SpawnCharacter(Profile profile, Vector3 position, int playerId)
        {
            Logger.LogDebug($"{nameof(SpawnCharacter)}:{nameof(position)}:{position}");

            // If this is an actual PLAYER player that we're creating a drone for, when we set
            // aiControl to true then they'll automatically run voice lines (eg when throwing
            // a grenade) so we need to make sure it's set to FALSE for the drone version of them.
            var useAiControl = ProfileIdsAI.Contains(profile.Id);

            // For actual bots, we can gain SIGNIFICANT clientside performance on the
            // non-host client by ENABLING aiControl for the bot. This has zero consequences
            // in terms of synchronization. No idea why having aiControl OFF is so expensive,
            // perhaps it's more accurate to think of it as an inverse bool of
            // "player controlled", where the engine has to enable a bunch of additional
            // logic when aiControl is turned off (in other words, for players)?


            var otherPlayer = CoopPlayer.Create(playerId
               , position
               , Quaternion.identity
               ,
               "Player",
               "ClientPlayer_" + profile.Nickname + "_"
               , EPointOfView.ThirdPerson
               , profile
               , aiControl: useAiControl
               , EUpdateQueue.Update
               , EFT.Player.EUpdateMode.Manual
               , EFT.Player.EUpdateMode.Auto
               // Cant use ObservedPlayerMode, it causes the player to fall through the floor and die
               , BackendConfigManager.Config.CharacterController.ObservedPlayerMode
               //, BackendConfigManager.Config.CharacterController.ClientPlayerMode
               , () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity
               , FilterCustomizationClass.Default
               , null
               , isYourPlayer: false
               , isClientDrone: true
               , initialMongoId: PlayerInventoryMongoIds[profile.Id]
               ).Result;

            if (otherPlayer == null)
                return null;

            otherPlayer.Position = position;
            otherPlayer.Transform.position = position;

            // ----------------------------------------------------------------------------------------------------
            // Add the player to the custom Players list
            if (!Players.ContainsKey(profile.ProfileId))
                Players.TryAdd(profile.ProfileId, (CoopPlayer)otherPlayer);

            PlayerClients.Add((CoopPlayerClient)otherPlayer);

            if (!Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.ProfileId == profile.ProfileId))
                Singleton<GameWorld>.Instance.RegisteredPlayers.Add(otherPlayer);

            if (!Singleton<GameWorld>.Instance.allAlivePlayersByID.ContainsKey(profile.ProfileId))
                Singleton<GameWorld>.Instance.RegisterPlayer(otherPlayer);


            if (!SpawnedPlayers.ContainsKey(profile.ProfileId))
                SpawnedPlayers.Add(profile.ProfileId, otherPlayer);

            // Create/Add PlayerReplicatedComponent to the LocalPlayer
            // This shouldn't be needed. Handled in CoopPlayer.Create code
            //var prc = otherPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
            //prc.IsClientDrone = true;

            if (!SITMatchmaking.IsClient)
            {
                if (ProfileIdsUser.Contains(otherPlayer.ProfileId))
                {
                    PlayersForAIToTarget.Enqueue(otherPlayer);
                }
            }

            if (useAiControl)
            {
                if (profile.Info.Side == EPlayerSide.Bear || profile.Info.Side == EPlayerSide.Usec)
                {
                    var backpackSlot = profile.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack);
                    var backpack = backpackSlot.ContainedItem;
                    if (backpack != null)
                    {
                        Item[] items = backpack.GetAllItems()?.ToArray();
                        if (items != null)
                        {
                            for (int i = 0; i < items.Count(); i++)
                            {
                                Item item = items[i];
                                if (item == backpack)
                                    continue;

                                item.SpawnedInSession = true;
                            }
                        }
                    }
                }
            }
            else if (profile.Info.Side != EPlayerSide.Savage) // Make Player PMC items are all not 'FiR'
            {
                Item[] items = profile.Inventory.AllRealPlayerItems?.ToArray();
                if (items != null)
                {
                    for (int i = 0; i < items.Count(); i++)
                    {
                        Item item = items[i];
                        item.SpawnedInSession = false;
                    }
                }
            }

            if (!SpawnedPlayersToFinalize.Any(x => otherPlayer))
                SpawnedPlayersToFinalize.Add(otherPlayer);

            Logger.LogDebug($"CreateLocalPlayer::{profile.Info.Nickname}::Spawned.");

            SetWeaponInHandsOfNewPlayer(otherPlayer, () => { });

            // Assign the SIT GroupId to the Users in the Raid
            if (ProfileIdsUser.Contains(otherPlayer.ProfileId))
                otherPlayer.Profile.Info.GroupId = "SIT";

            return otherPlayer;
        }

        /// <summary>
        /// Attempts to set up the New Player with the current weapon after spawning
        /// </summary>
        /// <param name="person"></param>
        public void SetWeaponInHandsOfNewPlayer(EFT.Player person, Action successCallback)
        {
            var equipment = person.Profile.Inventory.Equipment;
            if (equipment == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer: {person.Profile.ProfileId} has no Equipment!");
            }
            Item item = null;

            if (equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.Holster).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.Holster).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem;

            if (item == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to find any weapon for {person.Profile.ProfileId}");
            }

            person.SetItemInHands(item, (IResult) =>
            {

                if (IResult.Failed == true)
                {
                    Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to set item {item} in hands for {person.Profile.ProfileId}");
                }

                if (IResult.Succeed == true)
                {
                    if (successCallback != null)
                        successCallback();
                }

                if (person.TryGetItemInHands<Item>() != null)
                {
                    if (successCallback != null)
                        successCallback();
                }

            });
        }

        private Dictionary<string, PlayerHealthPacket> LastPlayerHealthPackets = new();

        private DateTime LastPlayerStateSent { get; set; } = DateTime.Now;
        public ulong LocalIndex { get; set; }

        public float LocalTime => 0;

        public BaseLocalGame<EftGamePlayerOwner> LocalGameInstance { get; internal set; }

        //public bool HighPingMode { get; set; } = false;
        public bool ServerHasStopped { get; set; }
        private bool ServerHasStoppedActioned { get; set; }
        public ConcurrentQueue<EFT.Player> PlayersForAIToTarget { get; } = new();
        public bool GameWorldGameStarted { get; private set; }
    }

    public enum ESpawnState
    {
        None = 0,
        Loading = 1,
        Spawning = 2,
        Spawned = 3,
        Ignore = 98,
        Error = 99,
    }

    public class SITConfig
    {
        public bool showPlayerNameTags { get; set; }

        /// <summary>
        /// Doesn't do anything
        /// </summary>

        public bool showPlayerNameTagsOnlyWhenVisible { get; set; }

        public bool showPlayerNameTagsForEnemies { get; set; } = false;

        public bool useClientSideDamageModel { get; set; } = false;
    }


}
