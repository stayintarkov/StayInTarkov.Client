using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using EFT.UI;
using JetBrains.Annotations;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Player;
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

namespace StayInTarkov.Coop
{
    /// <summary>
    /// Coop Game Component is the User 1-2-1 communication to the Server
    /// </summary>
    public class CoopGameComponent : MonoBehaviour, IFrameIndexer
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
        public ConcurrentDictionary<string, EFT.Player> Players { get; } = new();

        public ConcurrentDictionary<string, ObservedPlayerController> OtherPlayers { get; } = new();

        //public EFT.Player[] PlayerUsers
        public IEnumerable<EFT.Player> PlayerUsers
        {
            get
            {

                if (Players == null)
                    yield return null;

                var keys = Players.Keys.Where(x => x.StartsWith("pmc")).ToArray();
                foreach (var key in keys)
                    yield return Players[key];


            }
        }

        public EFT.Player[] PlayerBots
        {
            get
            {
                if (LocalGameInstance is CoopGame coopGame)
                {
                    if (MatchmakerAcceptPatches.IsClient || coopGame.Bots.Count == 0)
                        return Players.Values.Where(x => !x.ProfileId.StartsWith("pmc")).ToArray();

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

        public List<EFT.LocalPlayer> SpawnedPlayersToFinalize { get; private set; } = new();

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

            LCByterCheck[0] = StayInTarkovHelperConstants
                .SITTypes
                .Any(x => x.Name == 
                Encoding.UTF8.GetString(new byte[] { 0x4c, 0x65, 0x67, 0x61, 0x6c, 0x47, 0x61, 0x6d, 0x65, 0x43, 0x68, 0x65, 0x63, 0x6b }))
                ? (byte)0x1 : (byte)0x0;
        }



        /// <summary>
        /// Unity Component Start Method
        /// </summary>
        void Start()
        {
            Logger.LogDebug("CoopGameComponent:Start");
            //GameCamera = Camera.current;
            //GCHelpers.ClearGarbage(unloadAssets: false);
            //ActionPacketHandler = this.GetOrAddComponent<ActionPacketHandlerComponent>();
            //ActionPacketHandler = CoopPatches.CoopGameComponentParent.AddComponent<ActionPacketHandlerComponent>();


            // ----------------------------------------------------
            // Always clear "Players" when creating a new CoopGameComponent
            //Players = new ConcurrentDictionary<string, EFT.Player>();

            OwnPlayer = (LocalPlayer)Singleton<GameWorld>.Instance.MainPlayer;

            Players.TryAdd(OwnPlayer.ProfileId, OwnPlayer);

            //RequestingObj = AkiBackendCommunication.GetRequestInstance(true, Logger);
            RequestingObj = AkiBackendCommunication.GetRequestInstance(false, Logger);
            RequestingObj.PostJsonAsync<SITConfig>("/SIT/Config", "{}").ContinueWith(x =>
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


            Task.Run(() => ReadFromServerCharactersLoop());
            StartCoroutine(ProcessServerCharacters());
            //Task.Run(() => ReadFromServerLastActions());
            //Task.Run(() => ProcessFromServerLastActions());
            StartCoroutine(EverySecondCoroutine());

            Task.Run(() => PeriodicEnableDisableGC());

            ListOfInteractiveObjects = FindObjectsOfType<WorldInteractiveObject>();
            //PatchConstants.Logger.LogDebug($"Found {ListOfInteractiveObjects.Length} interactive objects");

            CoopPatches.EnableDisablePatches();

            Player_Init_Coop_Patch.SendPlayerDataToServer((LocalPlayer)Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer));

        }

        private long? lastMemory { get;set; }

        /// <summary>
        /// This clears out the RAM usage very effectively.
        /// </summary>
        /// <returns></returns>
        private async Task PeriodicEnableDisableGC()
        {
            var coopGame = LocalGameInstance as CoopGame;
            if (coopGame == null)
                return;

            int counter = 0;
            await Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(1000);

                    counter++;

                    //var myPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                    //if ((myPlayer != null && (myPlayer.HealthController.IsAlive && !myPlayer.Velocity.Equals(Vector3.zero))) && maxMoveCounter > 0)
                    //{
                    //    maxMoveCounter--;
                    //    continue;
                    //}

                    //if (counter == (60 * PluginConfigSettings.Instance.AdvancedSettings.SITGarbageCollectorIntervalMinutes))
                    //{
                    //    GCHelpers.EnableGC();
                    //}

                    //if (counter == (61 * PluginConfigSettings.Instance.AdvancedSettings.SITGarbageCollectorIntervalMinutes))
                    //{
                    //    GCHelpers.DisableGC(true);
                    //    counter = 0;
                    //}

                    var memory = GC.GetTotalMemory(false);
                    if (!lastMemory.HasValue)
                        lastMemory = memory;

                    long memoryThreshold = PluginConfigSettings.Instance.AdvancedSettings.SITGCMemoryThreshold;

                    if (lastMemory.HasValue && memory > lastMemory.Value + (memoryThreshold * 1024 * 1024))
                    {
                        Logger.LogDebug($"Current Memory Allocated:{memory / 1024 / 1024}mb");
                        lastMemory = memory;
                        Stopwatch sw = Stopwatch.StartNew();

                        GCHelpers.EnableGC();
                        if (PluginConfigSettings.Instance.AdvancedSettings.SITGCAggressiveClean)
                        {
                            GCHelpers.ClearGarbage(true, PluginConfigSettings.Instance.AdvancedSettings.SITGCClearAssets);
                        }
                        else
                        {
                            GC.GetTotalMemory(true);
                            GCHelpers.DisableGC(true);
                        }

                        var freedMemory = GC.GetTotalMemory(false);
                        Logger.LogDebug($"Freed {(freedMemory > 0 ? (freedMemory / 1024 / 1024) : 0)}mb in memory");
                        Logger.LogDebug($"Garbage Collection took {sw.ElapsedMilliseconds}ms");
                        sw.Stop();
                        sw = null;

                    }

                } while (RunAsyncTasks && PluginConfigSettings.Instance.AdvancedSettings.UseSITGarbageCollector);
            });
        }

        private IEnumerator EverySecondCoroutine()
        {
            var waitSeconds = new WaitForSeconds(1.0f);
            var coopGame = LocalGameInstance as CoopGame;
            if (coopGame == null)
                yield return null;

            while (RunAsyncTasks)
            {
                yield return waitSeconds;

                var playersToExtract = new List<string>();
                // TODO: Store the exfil point in the ExtractingPlayers dict, need it for timer
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
                    //LocalGameInstance.Stop(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, ExitStatus.Survived, "", 0);
                }

                var world = Singleton<GameWorld>.Instance;

                // Hide extracted Players
                foreach (var playerId in coopGame.ExtractedPlayers)
                {
                    var player = world.RegisteredPlayers.Find(x => x.ProfileId == playerId) as EFT.Player;
                    if (player == null)
                        continue;

                    AkiBackendCommunicationCoop.PostLocalPlayerData(player
                        , new Dictionary<string, object>() { { "Extracted", true } }
                        , true);

                    if (player.ActiveHealthController != null)
                    {
                        if (!player.ActiveHealthController.MetabolismDisabled)
                        {
                            player.ActiveHealthController.AddDamageMultiplier(0);
                            player.ActiveHealthController.SetDamageCoeff(0);
                            player.ActiveHealthController.DisableMetabolism();
                            player.ActiveHealthController.PauseAllEffects();

                            player.SwitchRenderer(false);
                        }
                    }
                    //Singleton<GameWorld>.Instance.UnregisterPlayer(player);
                    //GameObject.Destroy(player);
                }
            }
        }

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
                        GameObject.DestroyImmediate(prc);
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


        public enum EQuitState
        {
            NONE = -1,
            YouAreDead,
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

            if (coopGame.ExtractedPlayers == null)
                return quitState;

            var numberOfPlayersDead = PlayerUsers.Count(x => !x.HealthController.IsAlive);
            var numberOfPlayersAlive = PlayerUsers.Count(x => x.HealthController.IsAlive);
            var numberOfPlayersExtracted = coopGame.ExtractedPlayers.Count;

            if (PlayerUsers.Count() > 1)
            {
                if (PlayerUsers.Count() == numberOfPlayersDead)
                {
                    quitState = EQuitState.YourTeamIsDead;
                }
                else if (!Singleton<GameWorld>.Instance.MainPlayer.PlayerHealthController.IsAlive)
                {
                    quitState = EQuitState.YouAreDead;
                }
            }
            else if (PlayerUsers.Any(x => !x.HealthController.IsAlive))
            {
                quitState = EQuitState.YouAreDead;
            }

            if (
                numberOfPlayersAlive > 0
                &&
                (numberOfPlayersAlive == numberOfPlayersExtracted || PlayerUsers.Count() == numberOfPlayersExtracted)
                )
            {
                quitState = EQuitState.YourTeamHasExtracted;
            }
            else if (coopGame.ExtractedPlayers.Contains(Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
            {
                if (MatchmakerAcceptPatches.IsClient)
                    quitState = EQuitState.YouHaveExtractedOnlyAsClient;
                else if (MatchmakerAcceptPatches.IsServer)
                    quitState = EQuitState.YouHaveExtractedOnlyAsHost;
            }
            return quitState;
        }

        void Update()
        {
            GameCamera = Camera.current;

            foreach (var controller in OtherPlayers.Values)
            {
                controller.ManualUpdate();
            }

            if (!Singleton<ISITGame>.Instantiated)
                return;

            var quitState = GetQuitState();

            if (
                Input.GetKeyDown(KeyCode.F8)
                &&
                quitState != EQuitState.NONE
                && !RequestQuitGame
                )
            {
                RequestQuitGame = true;
                Singleton<ISITGame>.Instance.Stop(
                    Singleton<GameWorld>.Instance.MainPlayer.ProfileId
                    , Singleton<ISITGame>.Instance.MyExitStatus
                    , Singleton<ISITGame>.Instance.MyExitLocation
                    , 0);
                return;
            }

            if (!MatchmakerAcceptPatches.IsClient)
            {

            }

            if (ServerHasStopped && !ServerHasStoppedActioned)
            {
                ServerHasStoppedActioned = true;
                try
                {
                    // Allow server to configure whether to send hanging clients to "Run Through" or "Survived" status.
                    // Set RunThroughOnServerStop to false for "Survived" (new) behavior.
                    var exitStatus = PluginConfigSettings.Instance.CoopSettings.RunThroughOnServerStop ? ExitStatus.Runner : ExitStatus.Survived;
                    LocalGameInstance.Stop(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, exitStatus, "", 0);
                }
                catch { }
                return;
            }

            //var DateTimeStart = DateTime.Now;

            //if (!PluginConfigSettings.Instance.CoopSettings.ForceHighPingMode)
            //    HighPingMode = ServerPing > PING_LIMIT_HIGH && MatchmakerAcceptPatches.IsClient;


            if (ActionPackets == null)
                return;

            if (Players == null)
                return;

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (RequestingObj == null)
                return;

            List<Dictionary<string, object>> playerStates = new();
            if (LastPlayerStateSent < DateTime.Now.AddMilliseconds(-PluginConfigSettings.Instance.CoopSettings.SETTING_PlayerStateTickRateInMS))
            {
                //Logger.LogDebug("Creating PRC");

                foreach (var player in Players.Values)
                {
                    if (player == null)
                        continue;

                    if (!player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent prc))
                        continue;

                    if (prc.IsClientDrone)
                        continue;

                    if (!player.enabled)
                        continue;

                    if (!player.isActiveAndEnabled)
                        continue;


                    CreatePlayerStatePacketFromPRC(ref playerStates, player, prc);
                }


                //Logger.LogDebug(playerStates.SITToJson());
                RequestingObj.SendListDataToPool(string.Empty, playerStates);

                LastPlayerStateSent = DateTime.Now;
            }

            if (SpawnedPlayersToFinalize == null)
                return;

            List<EFT.LocalPlayer> SpawnedPlayersToRemoveFromFinalizer = new();
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
            }

            if (Singleton<PreloaderUI>.Instantiated && LCByterCheck[0] == 0 && LCByterCheck[1] == 0)
            {
                LCByterCheck[1] = 1;
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("", StayInTarkovPlugin.IllegalMessage, ErrorScreen.EButtonType.QuitButton, 60, () => { Application.Quit(); }, () => { Application.Quit(); });
            }
        }

        byte[] LCByterCheck { get; } = new byte[2] { 0, 0 };

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
                        this.ServerHasStopped = true;
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
                //LocalPlayer otherPlayer = CreateLocalPlayer(profile, position, playerId);

                SpawnMessage spawnMessage = new()
                {
                    Side = profile.Side,
                    IsAI = false,
                    NickName = profile.Nickname,
                    AccountId = profile.AccountId,
                    Voice = profile.Info.Voice,
                    ProfileID = profile.Id,
                    Inventory = profile.Inventory,
                    HandsController = new() { HandControllerType = EHandsControllerType.Empty, FastHide = false, Armed = false, MalfunctionState = Weapon.EMalfunctionState.None, DrawAnimationSpeedMultiplier = 1f },
                    Customization = profile.Customization,
                    BodyPosition = position,
                    ArmorsInfo = [],
                    WildSpawnType = WildSpawnType.pmcBot,
                    VoIPState = EFT.Player.EVoipState.NotAvailable
                };

                var controller = ObservedPlayerController.CreateInstance<ObservedPlayerController, ObservedPlayerView>(playerId, spawnMessage);
                controller.StateContext.PlayerAnimator.SetIsThirdPerson(false);

                var prc = controller.PlayerView.GetOrAddComponent<PlayerReplicatedComponent>();

                OtherPlayers.TryAdd(profile.ProfileId, controller);

                //Singleton<GameWorld>.Instance.allObservedPlayersByID.Add(profile.ProfileId, controller.PlayerView);

                // TODO: I would like to use the following, but it causes the drones to spawn without a weapon.
                //CreateLocalPlayerAsync(profile, position, playerId);

                if (isDead)
                {
                    // Logger.LogDebug($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: CreatePhysicalOtherPlayerOrBot::Killing localPlayer with ID {playerId}");
                    //otherPlayer.ActiveHealthController.Kill(EDamageType.Undefined);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

        }

        private LocalPlayer CreateLocalPlayer(Profile profile, Vector3 position, int playerId)
        {
            // If this is an actual PLAYER player that we're creating a drone for, when we set
            // aiControl to true then they'll automatically run voice lines (eg when throwing
            // a grenade) so we need to make sure it's set to FALSE for the drone version of them.
            var useAiControl = !profile.Id.StartsWith("pmc");

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
               ""
               , EPointOfView.ThirdPerson
               , profile
               , aiControl: useAiControl
               , EUpdateQueue.Update
               , EFT.Player.EUpdateMode.Auto
               , EFT.Player.EUpdateMode.Auto
               , BackendConfigManager.Config.CharacterController.ClientPlayerMode
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
               , FilterCustomizationClass.Default
               , null
               , isYourPlayer: false
               , isClientDrone: true
               ).Result;


            if (otherPlayer == null)
                return null;

            // ----------------------------------------------------------------------------------------------------
            // Add the player to the custom Players list
            if (!Players.ContainsKey(profile.ProfileId))
                Players.TryAdd(profile.ProfileId, otherPlayer);

            if (!Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.ProfileId == profile.ProfileId))
                Singleton<GameWorld>.Instance.RegisteredPlayers.Add(otherPlayer);

            if (!SpawnedPlayers.ContainsKey(profile.ProfileId))
                SpawnedPlayers.Add(profile.ProfileId, otherPlayer);

            // Create/Add PlayerReplicatedComponent to the LocalPlayer
            // This shouldn't be needed. Handled in CoopPlayer.Create code
            var prc = otherPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
            prc.IsClientDrone = true;

            if (!MatchmakerAcceptPatches.IsClient)
            {
                if (otherPlayer.ProfileId.StartsWith("pmc"))
                {
                    if (LocalGameInstance != null)
                    {
                        var botController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BaseLocalGame<GamePlayerOwner>), typeof(BotsController)).GetValue(this.LocalGameInstance);
                        if (botController != null)
                        {
                            Logger.LogDebug("Adding Client Player to Enemy list");
                            botController.AddActivePLayer(otherPlayer);
                        }
                    }
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
            else // Make Player PMC items are all not 'FiR'
            {
                Item[] items = profile.Inventory.AllPlayerItems?.ToArray();
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

            SpawnMessage spawnMessage = new SpawnMessage()
            {
                Side = profile.Side,
                GroupID = otherPlayer.GroupId,
                TeamID = otherPlayer.TeamId,
                IsAI = otherPlayer.IsAI,
                NickName = profile.Nickname,
                AccountId = profile.AccountId,
                Voice = profile.Info.Voice,
                ProfileID = profile.Id,
                Inventory = profile.Inventory,
                HandsController = new() { HandControllerType = EHandsControllerType.Empty, FastHide = false, Armed = false, MalfunctionState = Weapon.EMalfunctionState.None, DrawAnimationSpeedMultiplier = 1f },
                Customization = profile.Customization,
                BodyPosition = otherPlayer.Position,
                ArmorsInfo = [],
                WildSpawnType = WildSpawnType.pmcBot,
                VoIPState = EFT.Player.EVoipState.NotAvailable
            };

            var controller = ObservedPlayerController.CreateInstance<ObservedPlayerController, ObservedPlayerView>(otherPlayer.PlayerId, spawnMessage);
            controller.ManualUpdate();
            OtherPlayers.TryAdd(profile.ProfileId, controller);

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

        private void CreatePlayerStatePacketFromPRC(ref List<Dictionary<string, object>> playerStates, EFT.Player player, PlayerReplicatedComponent prc)
        {
            Dictionary<string, object> dictPlayerState = new();

            // --- The important Ids
            dictPlayerState.Add("profileId", player.ProfileId);
            dictPlayerState.Add("serverId", GetServerId());

            // --- Positional 
            dictPlayerState.Add("pX", player.Position.x);
            dictPlayerState.Add("pY", player.Position.y);
            dictPlayerState.Add("pZ", player.Position.z);
            dictPlayerState.Add("rX", player.Rotation.x);
            dictPlayerState.Add("rY", player.Rotation.y);

            // --- Positional 
            dictPlayerState.Add("pose", player.MovementContext.PoseLevel);
            //dictPlayerState.Add("spd", player.MovementContext.CharacterMovementSpeed);
            dictPlayerState.Add("spr", player.Physical.Sprinting);
            //if (player.MovementContext.IsSprintEnabled)
            //{
            //    prc.ReplicatedDirection = new Vector2(1, 0);
            //}
            //dictPlayerState.Add("tp", prc.TriggerPressed);
            dictPlayerState.Add("alive", player.HealthController.IsAlive);
            dictPlayerState.Add("tilt", player.MovementContext.Tilt);
            dictPlayerState.Add("prn", player.MovementContext.IsInPronePose);

            dictPlayerState.Add("t", DateTime.Now.Ticks.ToString("G"));
            // ---------- 
            //dictPlayerState.Add("p.hs.c", player.Physical.HandsStamina.Current);
            //dictPlayerState.Add("p.hs.t", player.Physical.HandsStamina.TotalCapacity.Value);
            //dictPlayerState.Add("p.s.c", player.Physical.Stamina.Current);
            //dictPlayerState.Add("p.s.t", player.Physical.Stamina.TotalCapacity.Value);
            //
            //if (prc.ReplicatedDirection.HasValue)
            //{
            //    dictPlayerState.Add("dX", prc.ReplicatedDirection.Value.x);
            //    dictPlayerState.Add("dY", prc.ReplicatedDirection.Value.y);
            //}

            // ---------- 
            /*
            if (player.PlayerHealthController != null)
            {
                foreach (var b in Enum.GetValues(typeof(EBodyPart)))
                {
                    var effects = player.PlayerHealthController
                        .GetAllActiveEffects((EBodyPart)b).Where(x => !x.ToString().Contains("Exist"))
                        .Select(x => x.ToString());

                    if (!effects.Any())
                        continue;

                    var k = "hE." + b.ToString();
                    //Logger.LogInfo(k);
                    //Logger.LogInfo(effects.ToJson());
                    dictPlayerState.Add(k, effects.ToJson());
                }

            }
            */
            // ---------- 
            if (player.HealthController.IsAlive)
            {
                foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                {
                    if (bodyPart == EBodyPart.Common)
                        continue;

                    var health = player.HealthController.GetBodyPartHealth(bodyPart);
                    dictPlayerState.Add($"hp.{bodyPart}", health.Current);
                    dictPlayerState.Add($"hp.{bodyPart}.m", health.Maximum);
                }

                dictPlayerState.Add("en", player.HealthController.Energy.Current);
                dictPlayerState.Add("hy", player.HealthController.Hydration.Current);
            }
            // ----------
            dictPlayerState.Add("m", "PlayerState");

            playerStates.Add(dictPlayerState);
        }

        private DateTime LastPlayerStateSent { get; set; } = DateTime.Now;
        public ulong LocalIndex { get; set; }

        public float LocalTime => 0;

        public BaseLocalGame<GamePlayerOwner> LocalGameInstance { get; internal set; }

        int GuiX = 10;
        int GuiWidth = 400;

        //public const int PING_LIMIT_HIGH = 125;
        //public const int PING_LIMIT_MID = 100;

        public int ServerPing { get; set; } = 1;
        public ConcurrentQueue<int> ServerPingSmooth { get; } = new();

        //public bool HighPingMode { get; set; } = false;
        public bool ServerHasStopped { get; set; }
        private bool ServerHasStoppedActioned { get; set; }

        GUIStyle middleLabelStyle;
        GUIStyle middleLargeLabelStyle;
        GUIStyle normalLabelStyle;

        void OnGUI()
        {


            if (normalLabelStyle == null)
            {
                normalLabelStyle = new GUIStyle(GUI.skin.label);
                normalLabelStyle.fontSize = 16;
                normalLabelStyle.fontStyle = FontStyle.Bold;
            }
            if (middleLabelStyle == null)
            {
                middleLabelStyle = new GUIStyle(GUI.skin.label);
                middleLabelStyle.fontSize = 18;
                middleLabelStyle.fontStyle = FontStyle.Bold;
                middleLabelStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (middleLargeLabelStyle == null)
            {
                middleLargeLabelStyle = new GUIStyle(middleLabelStyle);
                middleLargeLabelStyle.fontSize = 24;
            }

            var rect = new UnityEngine.Rect(GuiX, 5, GuiWidth, 100);

            rect.y = 5;
            GUI.Label(rect, $"SIT Coop: " + (MatchmakerAcceptPatches.IsClient ? "CLIENT" : "SERVER"));
            rect.y += 15;

            // PING ------
            GUI.contentColor = Color.white;
            GUI.contentColor = ServerPing >= AkiBackendCommunication.PING_LIMIT_HIGH ? Color.red : ServerPing >= AkiBackendCommunication.PING_LIMIT_MID ? Color.yellow : Color.green;
            GUI.Label(rect, $"RTT:{(ServerPing)}");
            rect.y += 15;
            GUI.Label(rect, $"Host RTT:{(ServerPing + AkiBackendCommunication.Instance.HostPing)}");
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


            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 13;

            var w = 0.5f; // proportional width (0..1)
            var h = 0.2f; // proportional height (0..1)
            var rectEndOfGameMessage = UnityEngine.Rect.zero;
            rectEndOfGameMessage.x = (float)(Screen.width * (1 - w)) / 2;
            rectEndOfGameMessage.y = (float)(Screen.height * (1 - h)) / 2 + (Screen.height / 3);
            rectEndOfGameMessage.width = Screen.width * w;
            rectEndOfGameMessage.height = Screen.height * h;

            var numberOfPlayersDead = PlayerUsers.Count(x => !x.HealthController.IsAlive);


            if (LocalGameInstance == null)
                return;

            var coopGame = LocalGameInstance as CoopGame;
            if (coopGame == null)
                return;

            rect = DrawSITStats(rect, numberOfPlayersDead, coopGame);

            var quitState = GetQuitState();
            switch (quitState)
            {
                case EQuitState.YourTeamIsDead:
                    //GUI.Label(rectEndOfGameMessage, $"You're team is Dead! Please quit now using the F8 Key.", middleLargeLabelStyle);
                    if (GUI.Button(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_DEAD"], middleLargeLabelStyle))
                    {

                    }
                    break;
                case EQuitState.YouAreDead:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD"], middleLargeLabelStyle);
                    break;
                case EQuitState.YourTeamHasExtracted:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_EXTRACTED"], middleLargeLabelStyle);
                    break;
                case EQuitState.YouHaveExtractedOnlyAsHost:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED"], middleLargeLabelStyle);
                    break;
                case EQuitState.YouHaveExtractedOnlyAsClient:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED_HOST"], middleLargeLabelStyle);
                    break;
            }

            //if(quitState != EQuitState.NONE)
            //{
            //    var rectEndOfGameButton = new Rect(rectEndOfGameMessage);
            //    rectEndOfGameButton.y += 15;
            //    if(GUI.Button(rectEndOfGameButton, "End Raid"))
            //    {

            //    }
            //}


            //OnGUI_DrawPlayerList(rect);
            OnGUI_DrawPlayerFriendlyTags(rect);
            //OnGUI_DrawPlayerEnemyTags(rect);

        }

        private Rect DrawSITStats(Rect rect, int numberOfPlayersDead, CoopGame coopGame)
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

        private void OnGUI_DrawPlayerFriendlyTags(UnityEngine.Rect rect)
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
                screenScale = (float)FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

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

                Vector3 aboveBotHeadPos = pl.PlayerBones.Pelvis.position + (Vector3.up * (pl.HealthController.IsAlive ? 1.1f : 0.3f));
                Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                if (screenPos.z > 0)
                {
                    rect.x = (screenPos.x * screenScale) - (rect.width / 2);
                    rect.y = Screen.height - ((screenPos.y + rect.height / 2) * screenScale);

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

        private void OnGUI_DrawPlayerEnemyTags(UnityEngine.Rect rect)
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
                screenScale = (float)FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

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

                Vector3 aboveBotHeadPos = pl.Position + (Vector3.up * (pl.HealthController.IsAlive ? 1.5f : 0.5f));
                Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                if (screenPos.z > 0)
                {
                    rect.x = (screenPos.x * screenScale) - (rect.width / 2);
                    rect.y = Screen.height - (screenPos.y * screenScale) - 15;

                    var distanceFromCamera = Math.Round(Vector3.Distance(Camera.current.gameObject.transform.position, pl.Position));
                    GUI.Label(rect, $"{pl.Profile.Nickname} {distanceFromCamera}m", middleLabelStyle);
                    rect.y += 15;
                    GUI.Label(rect, $"X", middleLabelStyle);
                }
            }
        }

        private void OnGUI_DrawPlayerList(UnityEngine.Rect rect)
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
