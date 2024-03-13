using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json.Linq;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Controllers.Health;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using StayInTarkov.EssentialPatches;
using StayInTarkov.Memory;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Rect = UnityEngine.Rect;
using BSGMemoryGC = GClass772;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    /// <summary>
    /// Coop Game Component is the User 1-2-1 communication to the Server. This can be seen as an extension component to CoopGame.
    /// </summary>
    public class CoopGameComponent : MonoBehaviour
    {
        #region Fields/Properties        
        public WorldInteractiveObject[] ListOfInteractiveObjects { get; set; }
        private AkiBackendCommunication RequestingObj { get; set; }
        public SITConfig SITConfig { get; private set; } = new SITConfig();
        public string ServerId { get; set; } = null;
        public long Timestamp { get; set; } = 0;

        public EFT.Player OwnPlayer { get; set; }

        /// <summary>
        /// ProfileId to Player instance
        /// </summary>
        public ConcurrentDictionary<string, CoopPlayer> Players { get; } = new();

        /// <summary>
        /// The Client Drones connected to this Raid
        /// </summary>
        public HashSet<CoopPlayerClient> PlayerClients { get; } = new();


        //public EFT.Player[] PlayerUsers
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

        BepInEx.Logging.ManualLogSource Logger { get; set; }
        public ConcurrentDictionary<string, ESpawnState> PlayersToSpawn { get; private set; } = new();
        public ConcurrentDictionary<string, Dictionary<string, object>> PlayersToSpawnPacket { get; private set; } = new();
        public Dictionary<string, Profile> PlayersToSpawnProfiles { get; private set; } = new();
        public ConcurrentDictionary<string, Vector3> PlayersToSpawnPositions { get; private set; } = new();

        public HashSet<string> ProfileIdsAI { get; } = new();
        public HashSet<string> ProfileIdsUser { get; } = new();

        public List<LocalPlayer> SpawnedPlayersToFinalize { get; private set; } = new();

        public BlockingCollection<Dictionary<string, object>> ActionPackets => ActionPacketHandler.ActionPackets;

        private Dictionary<string, object>[] m_CharactersJson { get; set; }

        public bool RunAsyncTasks { get; set; } = true;

        float screenScale = 1.0f;

        Camera GameCamera { get; set; }

        public ActionPacketHandlerComponent ActionPacketHandler { get; } = CoopPatches.CoopGameComponentParent.GetOrAddComponent<ActionPacketHandlerComponent>();

        #endregion

        #region Public Voids

        public static CoopGameComponent GetCoopGameComponent()
        {
            if (CoopPatches.CoopGameComponentParent == null)
                return null;

            var coopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
            if (coopGameComponent != null)
                return coopGameComponent;

            return null;
        }

        public static bool TryGetCoopGameComponent(out CoopGameComponent coopGameComponent)
        {
            coopGameComponent = GetCoopGameComponent();
            return coopGameComponent != null;
        }

        public static string GetServerId()
        {
            var coopGC = GetCoopGameComponent();
            if (coopGC == null)
                return null;

            return coopGC.ServerId;
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
            Logger = BepInEx.Logging.Logger.CreateLogSource("CoopGameComponent");
            Logger.LogDebug("CoopGameComponent:Awake");

            gameObject.AddComponent<CoopGameGUIComponent>();

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
            Logger.LogDebug("CoopGameComponent:Start");

            // Get Reference to own Player
            //OwnPlayer = (LocalPlayer)Singleton<GameWorld>.Instance.MainPlayer;

            //// Add own Player to Players list
            //Players.TryAdd(OwnPlayer.ProfileId, (CoopPlayer)OwnPlayer);

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

            // Run an immediate call to get characters in the server
            _ = ReadFromServerCharacters();

            // Get a Result of Characters within an interval loop
            _ = Task.Run(() => ReadFromServerCharactersLoop());

            // Process the Characters retrieved from the previous loop
            StartCoroutine(ProcessServerCharacters());

            // Run any methods you wish every second
            StartCoroutine(EverySecondCoroutine());

            StartCoroutine(SendPlayerStatePacket());

            // Get a List of Interactive Objects (this is a slow method), so run once here to maintain a reference
            ListOfInteractiveObjects = FindObjectsOfType<WorldInteractiveObject>();

            // Enable the Coop Patches
            CoopPatches.EnableDisablePatches();

            Singleton<GameWorld>.Instance.AfterGameStarted += GameWorld_AfterGameStarted;

            // In game ping system.
            if (Singleton<FrameMeasurer>.Instantiated)
            {
                FrameMeasurer instance = Singleton<FrameMeasurer>.Instance;
                instance.PlayerRTT = ServerPing;
                instance.ServerFixedUpdateTime = ServerPing;
                instance.ServerTime = ServerPing;
                instance.NetworkQuality.CreateMeasurers();
            }
        }

        private IEnumerator SendPlayerStatePacket()
        {
            using PlayerStatesPacket playerStatesPacket = new PlayerStatesPacket();
           
            List<PlayerStatePacket> packets = new List<PlayerStatePacket>();
            foreach (var player in Players.Values)
            {
                if (player == null)
                    continue;

                if (player is CoopPlayerClient)
                    continue;

                if (!player.TryGetComponent(out PlayerReplicatedComponent prc))
                    continue;

                if (prc.IsClientDrone)
                    continue;

                if (!player.enabled)
                    continue;

                if (!player.isActiveAndEnabled)
                    continue;

                CreatePlayerStatePacketFromPRC(ref packets, player);
            }

            //playerStates.Add("dataList", playerStateArray);
            //Logger.LogDebug(playerStates.SITToJson());
            playerStatesPacket.PlayerStates = packets.ToArray();
            var serialized = playerStatesPacket.Serialize();

            //Logger.LogDebug($"{nameof(playerStatesPacket)} is {serialized.Length} in Length");

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

            GameClient.SendData(serialized);

            LastPlayerStateSent = DateTime.Now;

            yield return new WaitForSeconds(PluginConfigSettings.Instance.CoopSettings.SETTING_PlayerStateTickRateInMS / 1000f);
            StartCoroutine(SendPlayerStatePacket());
        }

        private void GameWorld_AfterGameStarted()
        {
            GameWorldGameStarted = true;
            Logger.LogDebug(nameof(GameWorld_AfterGameStarted));
            if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any())
            {
                // Send My Player to Aki, so that other clients know about me
                CoopSITGame.SendPlayerDataToServer((LocalPlayer)Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer));
            }

            // Start the SIT Garbage Collector
            BSGMemoryGC.RunHeapPreAllocation();
            BSGMemoryGC.Collect(force: true);
            BSGMemoryGC.EmptyWorkingSet();
            BSGMemoryGC.GCEnabled = true;
            Resources.UnloadUnusedAssets();

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

                if (!Singleton<ISITGame>.Instantiated)
                    continue;

                if (!Singleton<GameWorld>.Instantiated)
                    continue;

                if (!GameWorldGameStarted)
                    continue;

                //Logger.LogDebug($"DEBUG: {nameof(EverySecondCoroutine)}");

                var coopGame = Singleton<ISITGame>.Instance;

                var playersToExtract = new HashSet<string>();
                foreach (var exfilPlayer in coopGame.ExtractingPlayers)
                {
                    var exfilTime = new TimeSpan(0, 0, (int)exfilPlayer.Value.Item1);
                    var timeInExfil = new TimeSpan(DateTime.Now.Ticks - exfilPlayer.Value.Item2);
                    if (timeInExfil >= exfilTime)
                    {
                        if (!playersToExtract.Contains(exfilPlayer.Key))
                        {
                            Logger.LogDebug(exfilPlayer.Key + " should extract");
                            playersToExtract.Add(exfilPlayer.Key);
                        }
                    }
                    else
                    {
                        Logger.LogDebug(exfilPlayer.Key + " extracting " + timeInExfil);

                    }
                }

                foreach (var player in playersToExtract)
                {
                    coopGame.ExtractingPlayers.Remove(player);
                    coopGame.ExtractedPlayers.Add(player);
                }

                var world = Singleton<GameWorld>.Instance;

                // Hide extracted Players
                foreach (var profileId in coopGame.ExtractedPlayers)
                {
                    var player = world.RegisteredPlayers.Find(x => x.ProfileId == profileId) as EFT.Player;
                    if (player == null)
                        continue;

                    if (!ExtractedProfilesSent.Contains(profileId))
                    {
                        ExtractedProfilesSent.Add(profileId);
                        AkiBackendCommunicationCoop.PostLocalPlayerData(player
                            , new Dictionary<string, object>() { { "m", "Extraction" }, { "Extracted", true } }
                            );
                    }

                    if (player.ActiveHealthController != null)
                    {
                        if (!player.ActiveHealthController.MetabolismDisabled)
                        {
                            player.ActiveHealthController.AddDamageMultiplier(0);
                            player.ActiveHealthController.SetDamageCoeff(0);
                            player.ActiveHealthController.DisableMetabolism();
                            player.ActiveHealthController.PauseAllEffects();

                            //player.SwitchRenderer(false);

                            // TODO: Currently. Destroying your own Player just breaks the game and it appears to be "frozen". Need to learn a new way to do a FreeCam!
                            //if (Singleton<GameWorld>.Instance.MainPlayer.ProfileId != profileId)
                            //    Destroy(player);
                        }
                    }
                }


                // Add players who have joined to the AI Enemy Lists
                var botController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BaseLocalGame<GamePlayerOwner>), typeof(BotsController)).GetValue(Singleton<ISITGame>.Instance);
                if (botController != null)
                {
                    while (PlayersForAIToTarget.TryDequeue(out var otherPlayer))
                    {
                        Logger.LogDebug($"Adding {otherPlayer.Profile.Nickname} to Enemy list");
                        botController.AddActivePLayer(otherPlayer);
                        botController.AddEnemyToAllGroups(otherPlayer, otherPlayer, otherPlayer);
                    }
                }

            }
        }

        private HashSet<string> ExtractedProfilesSent = new();

        void OnDestroy()
        {
            StayInTarkovHelperConstants.Logger.LogDebug($"CoopGameComponent:OnDestroy");

            if (Players != null)
            {
                foreach (var pl in Players)
                {
                    if (pl.Value == null)
                        continue;

                    if (pl.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                    {
                        DestroyImmediate(prc);
                    }
                }
            }
            Players.Clear();
            PlayersToSpawnProfiles.Clear();
            PlayersToSpawnPositions.Clear();
            PlayersToSpawnPacket.Clear();
            RunAsyncTasks = false;
            StopCoroutine(ProcessServerCharacters());
            StopCoroutine(EverySecondCoroutine());

            CoopPatches.EnableDisablePatches();
        }

        TimeSpan LateUpdateSpan = TimeSpan.Zero;
        Stopwatch swActionPackets { get; } = new Stopwatch();
        bool PerformanceCheck_ActionPackets { get; set; } = false;
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
                Input.GetKeyDown(KeyCode.F8)
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
                        Singleton<ISITGame>.Instance.Stop(
                            Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                            , Singleton<ISITGame>.Instance.MyExitStatus
                            , Singleton<ISITGame>.Instance.MyExitLocation
                            , 0);
                    }
                }
                else
                {
                    Singleton<ISITGame>.Instance.Stop(
                            Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                            , Singleton<ISITGame>.Instance.MyExitStatus
                            , Singleton<ISITGame>.Instance.MyExitLocation
                            , 0);
                }
                return;
            }
            else if (
                Input.GetKeyDown(KeyCode.F7)
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
                            Singleton<ISITGame>.Instance.Stop(
                                Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                                , Singleton<ISITGame>.Instance.MyExitStatus
                                , Singleton<ISITGame>.Instance.MyExitLocation
                                , 0);
                        }
                    }
                }
                return;
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
                    Singleton<ISITGame>.Instance.Stop(
                            Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                            , Singleton<ISITGame>.Instance.MyExitStatus
                            , Singleton<ISITGame>.Instance.MyExitLocation
                            , 0);
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

            if (ActionPackets == null)
                return;

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
                FrameMeasurer instance = Singleton<FrameMeasurer>.Instance;
                instance.PlayerRTT = ServerPing;
                instance.ServerFixedUpdateTime = ServerPing;
                instance.ServerTime = ServerPing;
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

        private async Task ReadFromServerCharactersLoop()
        {
            if (GetServerId() == null)
                return;


            while (RunAsyncTasks)
            {
                await Task.Delay(10000);

                if (Players == null)
                    continue;

                await ReadFromServerCharacters();

            }
        }

        private async Task ReadFromServerCharacters()
        {
            Dictionary<string, object> d = new();
            d.Add("serverId", GetServerId());
            d.Add("pL", new List<string>());

            // -----------------------------------------------------------------------------------------------------------
            // We must filter out characters that already exist on this match!
            //
            var playerList = new List<string>();
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
            {
                if (PlayersToSpawn.Count > 0)
                    playerList.AddRange(PlayersToSpawn.Keys.ToArray());
                if (Players.Keys.Any())
                    playerList.AddRange(Players.Keys.ToArray());
                if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any())
                    playerList.AddRange(Singleton<GameWorld>.Instance.RegisteredPlayers.Select(x => x.ProfileId));
                if (Singleton<GameWorld>.Instance.AllAlivePlayersList.Count > 0)
                    playerList.AddRange(Singleton<GameWorld>.Instance.AllAlivePlayersList.Select(x => x.ProfileId));
            }
            //
            // -----------------------------------------------------------------------------------------------------------
            // Ensure this is a distinct list of Ids
            d["pL"] = playerList.Distinct();
            var jsonDataToSend = d.ToJson();

            try
            {
                m_CharactersJson = await RequestingObj.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/read/players", jsonDataToSend, 30000);
                if (m_CharactersJson == null)
                    return;

                if (!m_CharactersJson.Any())
                    return;

                if (m_CharactersJson[0].ContainsKey("notFound"))
                {
                    // Game is broken and doesn't exist!
                    if (LocalGameInstance != null)
                    {
                        ServerHasStopped = true;
                    }
                    return;
                }

                //Logger.LogDebug($"CoopGameComponent.ReadFromServerCharacters:{actionsToValues.Length}");

                var packets = m_CharactersJson
                     .Where(x => x != null);
                if (packets == null)
                    return;

                foreach (var queuedPacket in packets)
                {
                    if (queuedPacket != null && queuedPacket.Count > 0)
                    {
                        if (queuedPacket != null)
                        {
                            if (queuedPacket.ContainsKey("m"))
                            {
                                var method = queuedPacket["m"].ToString();
                                if (method != "PlayerSpawn")
                                    continue;

                                string profileId = queuedPacket["profileId"].ToString();
                                if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
                                {
                                    if (Players == null
                                        || Players.ContainsKey(profileId)
                                        || Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.ProfileId == profileId)
                                        )
                                    {
                                        Logger.LogDebug($"Ignoring call to Spawn player {profileId}. The player already exists in the game.");
                                        continue;
                                    }
                                }

                                if (PlayersToSpawn.ContainsKey(profileId))
                                    continue;

                                if (!PlayersToSpawnPacket.ContainsKey(profileId))
                                    PlayersToSpawnPacket.TryAdd(profileId, queuedPacket);

                                if (!PlayersToSpawn.ContainsKey(profileId))
                                    PlayersToSpawn.TryAdd(profileId, ESpawnState.None);

                                if (queuedPacket.ContainsKey("isAI"))
                                    Logger.LogDebug($"{nameof(ReadFromServerCharacters)}:isAI:{queuedPacket["isAI"]}");

                                if (queuedPacket.ContainsKey("isAI") && queuedPacket["isAI"].ToString() == "True" && !ProfileIdsAI.Contains(profileId))
                                {
                                    ProfileIdsAI.Add(profileId);
                                    Logger.LogDebug($"Added AI Character {profileId} to {nameof(ProfileIdsAI)}");
                                }

                                if (queuedPacket.ContainsKey("isAI") && queuedPacket["isAI"].ToString() == "False" && !ProfileIdsUser.Contains(profileId))
                                {
                                    ProfileIdsUser.Add(profileId);
                                    Logger.LogDebug($"Added User Character {profileId} to {nameof(ProfileIdsUser)}");
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.LogError(ex.ToString());

            }
            finally
            {

            }
        }

        private IEnumerator ProcessServerCharacters()
        {
            var waitEndOfFrame = new WaitForEndOfFrame();

            if (GetServerId() == null)
                yield return waitEndOfFrame;

            var waitSeconds = new WaitForSeconds(0.5f);

            while (RunAsyncTasks)
            {
                yield return waitSeconds;
                foreach (var p in PlayersToSpawn)
                {
                    // If not showing drones. Check whether the "Player" has been registered, if they have, then ignore the drone
                    if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGSpawnDronesOnServer)
                    {
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

                    Vector3 newPosition = Vector3.zero;
                    if (PlayersToSpawnPacket[p.Key].ContainsKey("sPx")
                        && PlayersToSpawnPacket[p.Key].ContainsKey("sPy")
                        && PlayersToSpawnPacket[p.Key].ContainsKey("sPz"))
                    {
                        string npxString = PlayersToSpawnPacket[p.Key]["sPx"].ToString();
                        newPosition.x = float.Parse(npxString);
                        string npyString = PlayersToSpawnPacket[p.Key]["sPy"].ToString();
                        newPosition.y = float.Parse(npyString);
                        string npzString = PlayersToSpawnPacket[p.Key]["sPz"].ToString();
                        newPosition.z = float.Parse(npzString) + 0.5f;
                        ProcessPlayerBotSpawn(PlayersToSpawnPacket[p.Key], p.Key, newPosition, false);
                    }
                    else
                    {
                        Logger.LogError($"ReadFromServerCharacters::PlayersToSpawnPacket does not have positional data for {p.Key}");
                    }
                }


                yield return waitEndOfFrame;
            }
        }

        private void ProcessPlayerBotSpawn(Dictionary<string, object> packet, string profileId, Vector3 newPosition, bool isBot)
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


            // If CreatePhysicalOtherPlayerOrBot has been done before. Then ignore the Deserialization section and continue.
            if (PlayersToSpawn.ContainsKey(profileId)
                && PlayersToSpawnProfiles.ContainsKey(profileId)
                && PlayersToSpawnProfiles[profileId] != null
                )
            {
                var isDead = false;
                if (packet.ContainsKey("isDead"))
                {
                    Logger.LogDebug($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: Packet for {profileId} contains DEATH message, registered handling of this on spawn");
                    isDead = bool.Parse(packet["isDead"].ToString());
                }
                CreatePhysicalOtherPlayerOrBot(PlayersToSpawnProfiles[profileId], newPosition, isDead);
                return;
            }

            if (PlayersToSpawnProfiles.ContainsKey(profileId))
                return;

            PlayersToSpawnProfiles.Add(profileId, null);

            Logger.LogDebug($"ProcessPlayerBotSpawn:{profileId}");

            Profile profile = new();
            if (packet.ContainsKey("profileJson"))
            {
                if (packet["profileJson"].ToString().TrySITParseJson(out profile))
                {
                    //Logger.LogInfo("Obtained Profile");
                    profile.Skills.StartClientMode();
                    // Send to be loaded
                    PlayersToSpawnProfiles[profileId] = profile;
                }
                else
                {
                    Logger.LogError("Unable to Parse Profile");
                    PlayersToSpawn[profileId] = ESpawnState.Error;
                    return;
                }
            }
        }

        private void CreatePhysicalOtherPlayerOrBot(Profile profile, Vector3 position, bool isDead = false)
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
                LocalPlayer otherPlayer = CreateLocalPlayer(profile, position, playerId);
                if(otherPlayer == null)
                {
                    PlayersToSpawn[profile.ProfileId] = ESpawnState.Spawning;
                    return;
                }
                // TODO: I would like to use the following, but it causes the drones to spawn without a weapon.
                //CreateLocalPlayerAsync(profile, position, playerId);

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

        private LocalPlayer CreateLocalPlayer(Profile profile, Vector3 position, int playerId)
        {
            Logger.LogInfo($"{nameof(CreateLocalPlayer)}:{nameof(position)}:{position}");

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


            //var otherPlayer = LocalPlayer.Create(playerId
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
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
               , FilterCustomizationClass.Default
               , null
               , isYourPlayer: false
               , isClientDrone: true
               ).Result;

            if (otherPlayer == null)
                return null;

            otherPlayer.Position = position + new Vector3(0, 1, 0);
            otherPlayer.Transform.position = position + new Vector3(0, 1, 0);

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
            var prc = otherPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
            prc.IsClientDrone = true;

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
            else if(profile.Info.Side != EPlayerSide.Savage) // Make Player PMC items are all not 'FiR'
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

        private void CreatePlayerStatePacketFromPRC(ref List<PlayerStatePacket> playerStates, EFT.Player player)
        {
            // What this does is create a ISITPacket for the Character's health that can be SIT Serialized.
            PlayerHealthPacket playerHealth = new PlayerHealthPacket(player.ProfileId);
            playerHealth.Method = "53xMOD";
            playerHealth.IsAlive = player.HealthController.IsAlive;
            playerHealth.Energy = player.HealthController.Energy.Current;
            playerHealth.Hydration = player.HealthController.Hydration.Current;
            var bpIndex = 0;
            // Iterate over the BodyParts
            foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
            {
                var health = player.HealthController.GetBodyPartHealth(bodyPart);
                playerHealth.BodyParts[bpIndex] = new PlayerBodyPartHealthPacket();
                playerHealth.BodyParts[bpIndex].BodyPart = bodyPart;
                playerHealth.BodyParts[bpIndex].Current = health.Current;
                playerHealth.BodyParts[bpIndex].Maximum = health.Maximum;
                bpIndex++;
            }
            if (player.HealthController is SITHealthController sitHealthController)
            {
                // Paulov: TODO: Continue from here in another branch
                var tmpHealthEffectPacketList = new List<PlayerHealthEffectPacket>();
                while (sitHealthController.PlayerHealthEffectPackets.TryDequeue(out var p))
                {
                    tmpHealthEffectPacketList.Add(p);
                }
                playerHealth.HealthEffectPackets = tmpHealthEffectPacketList.ToArray();
            }

            if (playerHealth != null)
            {
                if (!LastPlayerHealthPackets.ContainsKey(player.ProfileId))
                    LastPlayerHealthPackets.Add(player.ProfileId, playerHealth);

                LastPlayerHealthPackets[player.ProfileId] = playerHealth;
            }

            // Create the ISITPacket for the Character's Current State
            PlayerStatePacket playerStatePacket = new PlayerStatePacket(
                player.ProfileId
                , player.Position
                , player.Rotation
                , player.HeadRotation
                , player.MovementContext.MovementDirection
                , player.MovementContext.CurrentState.Name
                , player.MovementContext.Tilt
                , player.MovementContext.Step
                , player.MovementContext.CurrentAnimatorStateIndex
                , player.MovementContext.CharacterMovementSpeed
                , player.MovementContext.IsInPronePose
                , player.MovementContext.PoseLevel
                , player.MovementContext.IsSprintEnabled
                , player.InputDirection
                , player.MovementContext.LeftStanceController.LastAnimValue
                , playerHealth
                , player.Physical.SerializationStruct
                , player.MovementContext.BlindFire
                , player.MovementContext.ActualLinearVelocity
                );
            ;

            // Add the serialized packets to the PlayerStates JArray
            playerStates.Add(playerStatePacket);

        }

        private DateTime LastPlayerStateSent { get; set; } = DateTime.Now;
        public ulong LocalIndex { get; set; }

        public float LocalTime => 0;

        public BaseLocalGame<GamePlayerOwner> LocalGameInstance { get; internal set; }

        int GuiX = 10;
        int GuiWidth = 400;

        //public const int PING_LIMIT_HIGH = 125;
        //public const int PING_LIMIT_MID = 100;

        public int ServerPing { get; private set; } = 1;
        public ConcurrentQueue<int> ServerPingSmooth { get; } = new();

        public void UpdatePing(int ms)
        {
            //Logger.LogDebug($"{nameof(UpdatePing)}:Updating with:{ms}");

            if (ServerPingSmooth.Count > 60)
                ServerPingSmooth.TryDequeue(out _);
            ServerPingSmooth.Enqueue(ms);
            ServerPing = ServerPingSmooth.Count > 0 ? (int)Math.Round(ServerPingSmooth.Average()) : 1;
        }

        //public bool HighPingMode { get; set; } = false;
        public bool ServerHasStopped { get; set; }
        private bool ServerHasStoppedActioned { get; set; }
        public ConcurrentQueue<EFT.Player> PlayersForAIToTarget { get; } = new();
        public bool GameWorldGameStarted { get; private set; }

        GUIStyle middleLabelStyle;
        GUIStyle middleLargeLabelStyle;
        GUIStyle normalLabelStyle;

        //void OnGUI()
        //{


        //    if (normalLabelStyle == null)
        //    {
        //        normalLabelStyle = new GUIStyle(GUI.skin.label);
        //        normalLabelStyle.fontSize = 16;
        //        normalLabelStyle.fontStyle = FontStyle.Bold;
        //    }
        //    if (middleLabelStyle == null)
        //    {
        //        middleLabelStyle = new GUIStyle(GUI.skin.label);
        //        middleLabelStyle.fontSize = 18;
        //        middleLabelStyle.fontStyle = FontStyle.Bold;
        //        middleLabelStyle.alignment = TextAnchor.MiddleCenter;
        //    }
        //    if (middleLargeLabelStyle == null)
        //    {
        //        middleLargeLabelStyle = new GUIStyle(middleLabelStyle);
        //        middleLargeLabelStyle.fontSize = 24;
        //    }

        //    var rect = new Rect(GuiX, 5, GuiWidth, 100);
        //    rect = DrawPing(rect);

        //    GUIStyle style = GUI.skin.label;
        //    style.alignment = TextAnchor.MiddleCenter;
        //    style.fontSize = 13;

        //    var w = 0.5f; // proportional width (0..1)
        //    var h = 0.2f; // proportional height (0..1)
        //    var rectEndOfGameMessage = Rect.zero;
        //    rectEndOfGameMessage.x = (float)(Screen.width * (1 - w)) / 2;
        //    rectEndOfGameMessage.y = (float)(Screen.height * (1 - h)) / 2 + Screen.height / 3;
        //    rectEndOfGameMessage.width = Screen.width * w;
        //    rectEndOfGameMessage.height = Screen.height * h;

        //    var numberOfPlayersDead = PlayerUsers.Count(x => !x.HealthController.IsAlive);


        //    if (LocalGameInstance == null)
        //        return;

        //    var coopGame = LocalGameInstance as CoopGame;
        //    if (coopGame == null)
        //        return;

        //    rect = DrawSITStats(rect, numberOfPlayersDead, coopGame);

        //    var quitState = GetQuitState();
        //    switch (quitState)
        //    {
        //        case EQuitState.YourTeamIsDead:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_DEAD"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YouAreDead:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_SOLO"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YouAreDeadAsHost:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_HOST"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YouAreDeadAsClient:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_CLIENT"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YourTeamHasExtracted:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_EXTRACTED"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YouHaveExtractedOnlyAsHost:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED_HOST"], middleLargeLabelStyle);
        //            break;
        //        case EQuitState.YouHaveExtractedOnlyAsClient:
        //            GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED_CLIENT"], middleLargeLabelStyle);
        //            break;
        //    }

        //    //if(quitState != EQuitState.NONE)
        //    //{
        //    //    var rectEndOfGameButton = new Rect(rectEndOfGameMessage);
        //    //    rectEndOfGameButton.y += 15;
        //    //    if(GUI.Button(rectEndOfGameButton, "End Raid"))
        //    //    {

        //    //    }
        //    //}


        //    //OnGUI_DrawPlayerList(rect);
        //    OnGUI_DrawPlayerFriendlyTags(rect);
        //    //OnGUI_DrawPlayerEnemyTags(rect);

        //}

        private Rect DrawPing(Rect rect)
        {
            if (!PluginConfigSettings.Instance.CoopSettings.ShowPing)
                return rect;

            rect.y = 5;
            GUI.Label(rect, $"SIT Coop: " + (SITMatchmaking.IsClient ? "CLIENT" : "SERVER"));
            rect.y += 15;

            // PING ------
            GUI.contentColor = Color.white;
            GUI.contentColor = ServerPing >= AkiBackendCommunication.PING_LIMIT_HIGH ? Color.red : ServerPing >= AkiBackendCommunication.PING_LIMIT_MID ? Color.yellow : Color.green;
            GUI.Label(rect, $"RTT:{ServerPing}");
            rect.y += 15;
            GUI.Label(rect, $"Host RTT:{ServerPing + AkiBackendCommunication.Instance.HostPing}");
            rect.y += 15;
            GUI.contentColor = Color.white;

            if (PerformanceCheck_ActionPackets)
            {
                GUI.contentColor = Color.red;
                GUI.Label(rect, $"BAD PERFORMANCE!");
                GUI.contentColor = Color.white;
                rect.y += 15;
            }

            if (AkiBackendCommunication.Instance.HighPingMode)
            {
                GUI.contentColor = Color.red;
                GUI.Label(rect, $"!HIGH PING MODE!");
                GUI.contentColor = Color.white;
                rect.y += 15;
            }

            return rect;
        }

        private Rect DrawSITStats(Rect rect, int numberOfPlayersDead, CoopSITGame coopGame)
        {
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_ShowSITStatistics)
                return rect;

            var numberOfPlayersAlive = PlayerUsers.Count(x => x.HealthController.IsAlive);
            // gathering extracted
            var numberOfPlayersExtracted = coopGame.ExtractedPlayers.Count;
            GUI.Label(rect, $"Players (Alive): {numberOfPlayersAlive}");
            rect.y += 15;
            GUI.Label(rect, $"Players (Dead): {numberOfPlayersDead}");
            rect.y += 15;
            GUI.Label(rect, $"Players (Extracted): {numberOfPlayersExtracted}");
            rect.y += 15;
            GUI.Label(rect, $"Bots: {PlayerBots.Length}");
            rect.y += 15;
            return rect;
        }

        private void OnGUI_DrawPlayerFriendlyTags(Rect rect)
        {
            if (SITConfig == null)
            {
                Logger.LogError("SITConfig is null?");
                return;
            }

            if (!SITConfig.showPlayerNameTags)
            {
                return;
            }

            if (FPSCamera.Instance == null)
                return;

            if (Players == null)
                return;

            if (PlayerUsers == null)
                return;

            if (Camera.current == null)
                return;

            if (!Singleton<GameWorld>.Instantiated)
                return;


            if (FPSCamera.Instance.SSAA != null && FPSCamera.Instance.SSAA.isActiveAndEnabled)
                screenScale = FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

            var ownPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (ownPlayer == null)
                return;

            foreach (var pl in PlayerUsers)
            {
                if (pl == null)
                    continue;

                if (pl.HealthController == null)
                    continue;

                if (pl.IsYourPlayer && pl.HealthController.IsAlive)
                    continue;

                Vector3 aboveBotHeadPos = pl.PlayerBones.Pelvis.position + Vector3.up * (pl.HealthController.IsAlive ? 1.1f : 0.3f);
                Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                if (screenPos.z > 0)
                {
                    rect.x = screenPos.x * screenScale - rect.width / 2;
                    rect.y = Screen.height - (screenPos.y + rect.height / 2) * screenScale;

                    GUIStyle labelStyle = middleLabelStyle;
                    labelStyle.fontSize = 14;
                    float labelOpacity = 1;
                    float distanceToCenter = Vector3.Distance(screenPos, new Vector3(Screen.width, Screen.height, 0) / 2);

                    if (distanceToCenter < 100)
                    {
                        labelOpacity = distanceToCenter / 100;
                    }

                    if (ownPlayer.HandsController.IsAiming)
                    {
                        labelOpacity *= 0.5f;
                    }

                    if (pl.HealthController.IsAlive)
                    {
                        var maxHealth = pl.HealthController.GetBodyPartHealth(EBodyPart.Common).Maximum;
                        var currentHealth = pl.HealthController.GetBodyPartHealth(EBodyPart.Common).Current / maxHealth;
                        labelStyle.normal.textColor = new Color(2.0f * (1 - currentHealth), 2.0f * currentHealth, 0, labelOpacity);
                    }
                    else
                    {
                        labelStyle.normal.textColor = new Color(255, 0, 0, labelOpacity);
                    }

                    var distanceFromCamera = Math.Round(Vector3.Distance(Camera.current.gameObject.transform.position, pl.Position));
                    GUI.Label(rect, $"{pl.Profile.Nickname} {distanceFromCamera}m", labelStyle);
                }
            }
        }

        private void OnGUI_DrawPlayerEnemyTags(Rect rect)
        {
            if (SITConfig == null)
            {
                Logger.LogError("SITConfig is null?");
                return;
            }

            if (!SITConfig.showPlayerNameTagsForEnemies)
            {
                return;
            }

            if (FPSCamera.Instance == null)
                return;

            if (Players == null)
                return;

            if (PlayerUsers == null)
                return;

            if (Camera.current == null)
                return;

            if (!Singleton<GameWorld>.Instantiated)
                return;


            if (FPSCamera.Instance.SSAA != null && FPSCamera.Instance.SSAA.isActiveAndEnabled)
                screenScale = FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

            var ownPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (ownPlayer == null)
                return;

            foreach (var pl in PlayerBots)
            {
                if (pl == null)
                    continue;

                if (pl.HealthController == null)
                    continue;

                if (!pl.HealthController.IsAlive)
                    continue;

                Vector3 aboveBotHeadPos = pl.Position + Vector3.up * (pl.HealthController.IsAlive ? 1.5f : 0.5f);
                Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                if (screenPos.z > 0)
                {
                    rect.x = screenPos.x * screenScale - rect.width / 2;
                    rect.y = Screen.height - screenPos.y * screenScale - 15;

                    var distanceFromCamera = Math.Round(Vector3.Distance(Camera.current.gameObject.transform.position, pl.Position));
                    GUI.Label(rect, $"{pl.Profile.Nickname} {distanceFromCamera}m", middleLabelStyle);
                    rect.y += 15;
                    GUI.Label(rect, $"X", middleLabelStyle);
                }
            }
        }

        private void OnGUI_DrawPlayerList(Rect rect)
        {
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_DEBUGShowPlayerList)
                return;

            rect.y += 15;

            if (PlayersToSpawn.Any(p => p.Value != ESpawnState.Spawned))
            {
                GUI.Label(rect, $"Spawning Players:");
                rect.y += 15;
                foreach (var p in PlayersToSpawn.Where(p => p.Value != ESpawnState.Spawned))
                {
                    GUI.Label(rect, $"{p.Key}:{p.Value}");
                    rect.y += 15;
                }
            }

            if (Singleton<GameWorld>.Instance != null)
            {
                var players = Singleton<GameWorld>.Instance.RegisteredPlayers.ToList();
                players.AddRange(Players.Values);
                players = players.Distinct(x => x.ProfileId).ToList();

                rect.y += 15;
                GUI.Label(rect, $"Players [{players.Count}]:");
                rect.y += 15;
                foreach (var p in players)
                {
                    GUI.Label(rect, $"{p.Profile.Nickname}:{(p.IsAI ? "AI" : "Player")}:{(p.HealthController.IsAlive ? "Alive" : "Dead")}");
                    rect.y += 15;
                }

                players.Clear();
                players = null;
            }
        }
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
