/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */


using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.Bots;
using EFT.EnvironmentEffect;
using EFT.Game.Spawning;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.MovingPlatforms;
using EFT.UI;
using EFT.UI.Screens;
using EFT.Weather;
using JsonType;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.FreeCamera;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Players;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace StayInTarkov.Coop.SITGameModes
{
    /// <summary>
    /// A custom Game Type
    /// </summary>
    internal sealed class CoopSITGame : BaseLocalGame<GamePlayerOwner>, IBotGame, ISITGame
    {
        public new bool InRaid { get { return true; } }


        public ISession BackEndSession { get { return StayInTarkovHelperConstants.BackEndSession; } }

        BotsController IBotGame.BotsController
        {
            get
            {
                if (BotsController == null)
                {
                    BotsController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(BotsController)).GetValue(this);
                }
                return BotsController;
            }
        }

        private static BotsController BotsController;

        public BotsController PBotsController
        {
            get
            {
                if (BotsController == null)
                {
                    BotsController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(BotsController)).GetValue(this);
                }
                return BotsController;
            }
        }

        public IWeatherCurve WeatherCurve
        {
            get
            {
                if (WeatherController.Instance != null)
                    return new WeatherCurve(new WeatherClass[1] { new() });

                return null;
            }
        }

        public EndByExitTrigerScenario EndByExitTrigerScenario
        {
            get
            {
                return ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(EndByExitTrigerScenario)).GetValue(this) as EndByExitTrigerScenario;
            }
        }

        public EndByTimerScenario EndByTimerScenario
        {
            get
            {
                return ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(EndByTimerScenario)).GetValue(this) as EndByTimerScenario;
            }
        }

        public DateTime GameWorldTime { get; set; }


        private static ManualLogSource Logger;

        internal static CoopSITGame Create(
            InputTree inputTree
            , Profile profile
            , GameDateTime backendDateTime
            , InsuranceCompanyClass insurance
            , MenuUI menuUI
            , CommonUI commonUI
            , PreloaderUI preloaderUI
            , GameUI gameUI
            , LocationSettingsClass.Location location
            , TimeAndWeatherSettings timeAndWeather
            , WavesSettings wavesSettings
            , EDateTime dateTime
            , Callback<ExitStatus, TimeSpan, MetricsClass> callback
            , float fixedDeltaTime
            , EUpdateQueue updateQueue
            , ISession backEndSession
            , TimeSpan sessionTime)
        {
            BotsController = null;

            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopSITGame));
            Logger.LogDebug("CoopGame.Create");
         

            //location.OfflineNewSpawn = false;
            //location.OfflineOldSpawn = true;
            //location.OldSpawn = true;

            CoopSITGame coopGame = 
                smethod_0<CoopSITGame>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));

#if DEBUG
            Logger.LogDebug($"DEBUG:{nameof(backendDateTime)}:{backendDateTime.ToJson()}");
#endif
            coopGame.GameWorldTime = backendDateTime.Boolean_0 ? backendDateTime.DateTime_1 : backendDateTime.DateTime_0;
            Logger.LogDebug($"DEBUG:{nameof(coopGame.GameWorldTime)}:{coopGame.GameWorldTime}");

            // ---------------------------------------------------------------------------------
            // Non Waves Scenario setup
            coopGame.nonWavesSpawnScenario_0 = NonWavesSpawnScenario.smethod_0(coopGame, location, coopGame.PBotsController);
            coopGame.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            // ---------------------------------------------------------------------------------
            // Waves Scenario setup
            coopGame.wavesSpawnScenario_0 = WavesSpawnScenario.smethod_0(
                    coopGame.gameObject
                    , location.waves
                    , new Action<BotSpawnWave>((wave) => coopGame.PBotsController.ActivateBotsByWave(wave))
                    , location);

            // ---------------------------------------------------------------------------------
            // Setup Boss Wave Manager
            coopGame.BossWaves = coopGame.FixBossWaveSettings(wavesSettings, location);
            var bosswavemanagerValue = BossWaveManager.smethod_0(coopGame.BossWaves, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }));
            coopGame.BossWaveManager = bosswavemanagerValue;

            coopGame.func_1 = (player) => GamePlayerOwner.Create<GamePlayerOwner>(player, inputTree, insurance, backEndSession, commonUI, preloaderUI, gameUI, coopGame.GameDateTime, location);

            // ---------------------------------------------------------------------------------
            // Setup ISITGame Singleton
            Singleton<ISITGame>.Create(coopGame);

            // ---------------------------------------------------------------------------------
            // Create Coop Game Component
            Logger.LogDebug($"{nameof(Create)}:Running {nameof(coopGame.CreateCoopGameComponent)}");
            coopGame.CreateCoopGameComponent();
            CoopGameComponent.GetCoopGameComponent().LocalGameInstance = coopGame;



            // ---------------------------------------------------------------------------------
            // Create GameClient(s)
            switch (SITMatchmaking.SITProtocol)
            {
                case ESITProtocol.RelayTcp:
                    coopGame.GameClient = coopGame.GetOrAddComponent<GameClientTCPRelay>();
                    break;
                default:
                    if (SITMatchmaking.IsServer)
                        coopGame.GameServer = coopGame.GetOrAddComponent<GameServerUDP>();

                    coopGame.GameClient = coopGame.GetOrAddComponent<GameClientUDP>();
                    break;
            }

            return coopGame;
        }

        public void CreateCoopGameComponent()
        {
            var coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent != null)
            {
                Destroy(coopGameComponent);
            }

            if (CoopPatches.CoopGameComponentParent != null)
            {
                Destroy(CoopPatches.CoopGameComponentParent);
                CoopPatches.CoopGameComponentParent = null;
            }

            if (CoopPatches.CoopGameComponentParent == null)
            {
                CoopPatches.CoopGameComponentParent = new GameObject("CoopGameComponentParent");
                DontDestroyOnLoad(CoopPatches.CoopGameComponentParent);
            }
            CoopPatches.CoopGameComponentParent.AddComponent<ActionPacketHandlerComponent>();
            coopGameComponent = CoopPatches.CoopGameComponentParent.AddComponent<CoopGameComponent>();
            coopGameComponent.LocalGameInstance = this;

            //coopGameComponent = gameWorld.GetOrAddComponent<CoopGameComponent>();
            if (!string.IsNullOrEmpty(SITMatchmaking.GetGroupId()))
            {
                Logger.LogDebug($"{nameof(CreateCoopGameComponent)}:{SITMatchmaking.GetGroupId()}");
                coopGameComponent.ServerId = SITMatchmaking.GetGroupId();
                coopGameComponent.Timestamp = SITMatchmaking.GetTimestamp();
            }
            else
            {
                Destroy(coopGameComponent);
                coopGameComponent = null;
                Logger.LogError("========== ERROR = COOP ========================");
                Logger.LogError("No Server Id found, Deleting Coop Game Component");
                Logger.LogError("================================================");
                throw new Exception("No Server Id found");
            }

            if (SITMatchmaking.IsServer)
            {
                //StartCoroutine(HostPinger());
                StartCoroutine(GameTimerSync());
                StartCoroutine(TimeAndWeatherSync());
                StartCoroutine(ArmoredTrainTimeSync());
            }

            StartCoroutine(ClientLoadingPinger());

            var friendlyAIJson = AkiBackendCommunication.Instance.GetJson($"/coop/server/friendlyAI/{CoopGameComponent.GetServerId()}");
            Logger.LogDebug(friendlyAIJson);
            //coopGame.FriendlyAIPMCSystem = JsonConvert.DeserializeObject<FriendlyAIPMCSystem>(friendlyAIJson);
        }

        private IEnumerator ClientLoadingPinger()
        {
            var waitSeconds = new WaitForSeconds(1f);

            while (true)
            {
                if (PlayerOwner == null)
                    yield return waitSeconds;

                // Send a message of nothing to keep the Socket Alive whilst loading
                AkiBackendCommunication.Instance.PostDownWebSocketImmediately("CLIENT_LOADING_KEEP_ALIVE");

                yield return waitSeconds;

            }
        }

        private IEnumerator DebugObjects()
        {
            var waitSeconds = new WaitForSeconds(10f);

            while (true)
            {
                if (PlayerOwner == null)
                    yield return waitSeconds;
                //foreach(var o in  .FindObjectsOfTypeAll(typeof(GameObject)))
                //{
                //   Logger.LogInfo(o.ToString());
                //}
                foreach (var c in PlayerOwner.Player.GetComponents(typeof(GameObject)))
                {
                    Logger.LogInfo(c.ToString());
                }
                yield return waitSeconds;

            }
        }

        private IEnumerator HostPinger()
        {
            var waitSeconds = new WaitForSeconds(1f);

            while (true)
            {
                yield return waitSeconds;

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    yield break;

                Dictionary<string, string> hostPingerPacket = new();
                hostPingerPacket.Add("HostPing", DateTime.UtcNow.Ticks.ToString());
                hostPingerPacket.Add("serverId", coopGameComponent.ServerId);
                Networking.GameClient.SendData(hostPingerPacket.ToJson());
            }
        }

        private IEnumerator GameTimerSync()
        {
            var waitSeconds = new WaitForSeconds(10f);

            while (true)
            {
                yield return waitSeconds;

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    yield break;

                if (GameTimer.StartDateTime.HasValue && GameTimer.SessionTime.HasValue)
                {
                    Dictionary<string, object> raidTimerDict = new()
                    {
                        { "serverId", coopGameComponent.ServerId },
                        { "m", "RaidTimer" },
                        { "sessionTime", (GameTimer.SessionTime - GameTimer.PastTime).Value.Ticks },
                    };
                    Networking.GameClient.SendData(raidTimerDict.ToJson());
                }
            }
        }

        private IEnumerator TimeAndWeatherSync()
        {
            var waitSeconds = new WaitForSeconds(15f);

            while (true)
            {
                yield return waitSeconds;

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    yield break;

                Dictionary<string, object> timeAndWeatherDict = new()
                {
                    { "serverId", coopGameComponent.ServerId },
                    { "m", "TimeAndWeather" }
                };

                if (GameDateTime != null)
                    timeAndWeatherDict.Add("GameDateTime", GameDateTime.Calculate().Ticks);

                var weatherController = WeatherController.Instance;
                if (weatherController != null)
                {
                    if (weatherController.CloudsController != null)
                        timeAndWeatherDict.Add("CloudDensity", weatherController.CloudsController.Density);

                    var weatherCurve = weatherController.WeatherCurve;
                    if (weatherCurve != null)
                    {
                        timeAndWeatherDict.Add("Fog", weatherCurve.Fog);
                        timeAndWeatherDict.Add("LightningThunderProbability", weatherCurve.LightningThunderProbability);
                        timeAndWeatherDict.Add("Rain", weatherCurve.Rain);
                        timeAndWeatherDict.Add("Temperature", weatherCurve.Temperature);
                        timeAndWeatherDict.Add("WindDirection.x", weatherCurve.Wind.x);
                        timeAndWeatherDict.Add("WindDirection.y", weatherCurve.Wind.y);
                        timeAndWeatherDict.Add("TopWindDirection.x", weatherCurve.TopWind.x);
                        timeAndWeatherDict.Add("TopWindDirection.y", weatherCurve.TopWind.y);
                    }

                    string packet = timeAndWeatherDict.ToJson();
                    Logger.LogDebug(packet);
                    Networking.GameClient.SendData(packet);
                }
            }
        }

        private IEnumerator ArmoredTrainTimeSync()
        {
            var waitSeconds = new WaitForSeconds(30f);

            while (true)
            {
                yield return waitSeconds;

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    yield break;

                // Make sure packet is only sent after the raid begins.
                if (GameTimer.StartDateTime.HasValue && GameTimer.SessionTime.HasValue)
                {
                    // Looking for Armored Train, if there is nothing, then we are not on the Reserve or Lighthouse.
                    Locomotive locomotive = FindObjectOfType<Locomotive>();
                    if (locomotive == null)
                        yield break;

                    // Utc time where the train will start moving.
                    FieldInfo departField = ReflectionHelpers.GetFieldFromType(typeof(MovingPlatform), "_depart");

                    Dictionary<string, object> dict = new()
                    {
                        { "serverId", coopGameComponent.ServerId },
                        { "m", "ArmoredTrainTime" },
                        { "utcTime", ((DateTime)departField.GetValue(locomotive)).Ticks },
                    };
                    Networking.GameClient.SendData(dict.ToJson());
                }
            }
        }

        private static int[] CultistSpawnTime = new [] { 6, 22 }; 

        private static bool CanSpawnCultist(int hour)
        {
            return hour <= CultistSpawnTime[0] || hour >= CultistSpawnTime[1];
        }

        public BossLocationSpawn[] FixBossWaveSettings(WavesSettings wavesSettings, LocationSettingsClass.Location location)
        {
            var bossLocationSpawns = location.BossLocationSpawn;
            if (!wavesSettings.IsBosses)
            {
                Logger.LogDebug($"{nameof(CoopSITGame)}:{nameof(FixBossWaveSettings)}: Bosses are disabled");
                return new BossLocationSpawn[0];
            }
            foreach (BossLocationSpawn bossLocationSpawn in bossLocationSpawns)
            {
#if DEBUG
                Logger.LogDebug($"{nameof(FixBossWaveSettings)}:===BEFORE===");
                Logger.LogDebug($"{nameof(FixBossWaveSettings)}:{bossLocationSpawn.ToJson()}");
#endif
                List<int> sourceEscortAmount;
                try
                {
                    sourceEscortAmount = bossLocationSpawn.BossEscortAmount.Split(',').Select(int.Parse).ToList();
                    bossLocationSpawn.ParseMainTypesTypes();
                }
                catch (Exception)
                {
                    Logger.LogError($"{nameof(CoopSITGame)}:{nameof(FixBossWaveSettings)}: Unable to parse BossEscortAmount");
                    continue;
                }
                float bossChance = bossLocationSpawn.BossChance;
#if DEBUG
                bossChance = 100f;
#endif
                if (CanSpawnCultist(GameWorldTime.Hour) && (bossLocationSpawn.BossType == WildSpawnType.sectantPriest || bossLocationSpawn.BossType == WildSpawnType.sectantWarrior))
                {
                    Logger.LogDebug($"Block spawn of Sectant (Cultist) in day time in hour {GameWorldTime.Hour}!");
                    bossChance = -1f;
                }
                bossLocationSpawn.BossChance = bossChance;
                bossLocationSpawn.BossEscortAmount = sourceEscortAmount != null ? sourceEscortAmount.Max((int x) => x).ToString() : "1";
                if(bossLocationSpawn.Supports == null && !string.IsNullOrEmpty(bossLocationSpawn.BossEscortType))
                {
                    Logger.LogDebug($"bossLocationSpawn.Supports is Null. Attempt to create them.");

                    bossLocationSpawn.Supports = new WildSpawnSupports[1];
                    bossLocationSpawn.Supports[0] = new WildSpawnSupports();
                    bossLocationSpawn.Supports[0].BossEscortDifficult = new [] { "normal" };
                    bossLocationSpawn.Supports[0].BossEscortAmount = 3;
                    if (Enum.TryParse<WildSpawnType>(bossLocationSpawn.BossEscortType, out var t))
                        bossLocationSpawn.Supports[0].BossEscortType = t;
                }
#if DEBUG
                Logger.LogDebug($"{nameof(FixBossWaveSettings)}:===AFTER===");
                Logger.LogDebug($"{nameof(FixBossWaveSettings)}:{bossLocationSpawn.ToJson()}");
#endif
            }
            return bossLocationSpawns;
        }

        public Dictionary<string, EFT.Player> Bots { get; } = new();

        private async Task<LocalPlayer> CreatePhysicalBot(Profile profile, Vector3 position)
        {
            if (SITMatchmaking.IsClient)
                return null;

            if (Bots != null && Bots.Count(x => x.Value != null && x.Value.PlayerHealthController.IsAlive) >= MaxBotCount)
            {
                Logger.LogDebug("Block spawn of Bot. Max Bot Count has been reached!");
                return null;
            }

            if (GameDateTime.Calculate().TimeOfDay < new TimeSpan(20, 0, 0) && profile.Info != null && profile.Info.Settings != null
                && (profile.Info.Settings.Role == WildSpawnType.sectantPriest || profile.Info.Settings.Role == WildSpawnType.sectantWarrior)
                )
            {
                Logger.LogDebug("Block spawn of Sectant (Cultist) in day time!");
                return null;
            }
            Logger.LogDebug($"CreatePhysicalBot: {profile.ProfileId}");

            LocalPlayer botPlayer;
            if (!Status.IsRunned())
            {
                botPlayer = null;
            }
            else if (Bots.ContainsKey(profile.Id))
            {
                botPlayer = null;
            }
            else
            {
                int num = 999 + Bots.Count;
                profile.SetSpawnedInSession(profile.Info.Side == EPlayerSide.Savage);

                botPlayer
                   = await CoopPlayer.Create(
                       num
                       , position
                       , Quaternion.identity
                       , "Player"
                       , ""
                       , EPointOfView.ThirdPerson
                       , profile
                       , true
                       , UpdateQueue
                       , EFT.Player.EUpdateMode.Manual
                       , EFT.Player.EUpdateMode.Auto
                       , BackendConfigManager.Config.CharacterController.BotPlayerMode
                    , () => 1f
                    , () => 1f
                    , FilterCustomizationClass1.Default

                    )
                  ;
                botPlayer.Location = Location_0.Id;
                if (Bots.ContainsKey(botPlayer.ProfileId))
                {
                    Destroy(botPlayer);
                    return null;
                }
                else
                {
                    Bots.Add(botPlayer.ProfileId, botPlayer);
                }

                // Start with SPT-AKI 3.7.0 AI PMC carrying 'FiR' items, this is just a simple "concept" of it.
                // Every AI PMC who have backpack, their items in the backpack are 'FiR'.
                if (profile.Info.Side == EPlayerSide.Bear || profile.Info.Side == EPlayerSide.Usec)
                {
                    var backpackSlot = profile.Inventory.Equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.Backpack);
                    var backpack = backpackSlot.ContainedItem;
                    if (backpack != null)
                    {
                        EFT.InventoryLogic.Item[] items = backpack.GetAllItems()?.ToArray();
                        if (items != null)
                        {
                            for (int i = 0; i < items.Count(); i++)
                            {
                                EFT.InventoryLogic.Item item = items[i];
                                if (item == backpack)
                                    continue;

                                item.SpawnedInSession = true;
                            }
                        }
                    }
                }

                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                {
                    Logger.LogDebug($"{nameof(CreatePhysicalBot)}:Unable to find {nameof(CoopGameComponent)}");
                    await Task.Delay(5000);
                }

                // 0.14 update. Add to ProfileIdsAI list.
                // Add to CoopGameComponent list
                coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)botPlayer);
                coopGameComponent.ProfileIdsAI.Add(profile.Id);

                SendPlayerDataToServer(botPlayer);

            }
            return botPlayer;
        }


        //public async Task<LocalPlayer> CreatePhysicalPlayer(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, QuestControllerClass questController)
        //{
        //    profile.SetSpawnedInSession(value: false);
        //    return await LocalPlayer.Create(playerId, position, rotation, "Player", "", EPointOfView.FirstPerson, profile, aiControl: false, base.UpdateQueue, armsUpdateMode, EFT.Player.EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.ClientPlayerMode, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity, new StatisticsManagerForPlayer1(), new FilterCustomizationClass(), questController, isYourPlayer: true);
        //}

        public string InfiltrationPoint;

        //public override void vmethod_0()
        //{
        //}

        /// <summary>
        /// Matchmaker countdown
        /// </summary>
        /// <param name="timeBeforeDeploy"></param>
        public override void vmethod_1(float timeBeforeDeploy)
        {

            base.vmethod_1(timeBeforeDeploy);
        }

        public static void SendOrReceiveSpawnPoint(ref ISpawnPoint selectedSpawnPoint, SpawnPoints spawnPoints)
        {
            var position = selectedSpawnPoint.Position;
            if (!SITMatchmaking.IsClient)
            {
                Dictionary<string, object> packet = new()
                {
                    {
                        "m",
                        "SpawnPointForCoop"
                    },
                    {
                        "serverId",
                        CoopGameComponent.GetServerId()
                    },
                    {
                        "x",
                        position.x
                    },
                    {
                        "y",
                        position.y
                    },
                    {
                        "z",
                        position.z
                    },
                    {
                        "id",
                        selectedSpawnPoint.Id
                    }
                };
                Logger.LogInfo("Setting Spawn Point to " + position);
                AkiBackendCommunication.Instance.PostJson("/coop/server/update", packet.ToJson());
                //var json = Request.Instance.GetJson($"/coop/server/spawnPoint/{CoopGameComponent.GetServerId()}");
                //Logger.LogInfo("Retreived Spawn Point " + json);
            }
            else if (SITMatchmaking.IsClient)
            {
                if (PluginConfigSettings.Instance.CoopSettings.AllPlayersSpawnTogether)
                {
                    var json = AkiBackendCommunication.Instance.GetJson($"/coop/server/spawnPoint/{CoopGameComponent.GetServerId()}");
                    Logger.LogInfo("Retreived Spawn Point " + json);
                    var retrievedPacket = json.ParseJsonTo<Dictionary<string, string>>();
                    var x = float.Parse(retrievedPacket["x"].ToString());
                    var y = float.Parse(retrievedPacket["y"].ToString());
                    var z = float.Parse(retrievedPacket["z"].ToString());
                    var teleportPosition = new Vector3(x, y, z);
                    selectedSpawnPoint = spawnPoints.First(x => x.Position == teleportPosition);
                }
            }
            //}
        }

        internal Dictionary<string, CoopPlayer> FriendlyPlayers { get; } = new Dictionary<string, CoopPlayer>();

        SpawnPoints spawnPoints = null;
        ISpawnPoint spawnPoint = null;

        /// <summary>
        /// Creating the EFT.LocalPlayer
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="layerName"></param>
        /// <param name="prefix"></param>
        /// <param name="pointOfView"></param>
        /// <param name="profile"></param>
        /// <param name="aiControl"></param>
        /// <param name="updateQueue"></param>
        /// <param name="armsUpdateMode"></param>
        /// <param name="bodyUpdateMode"></param>
        /// <param name="characterControllerMode"></param>
        /// <param name="getSensitivity"></param>
        /// <param name="getAimingSensitivity"></param>
        /// <param name="statisticsManager"></param>
        /// <param name="questController"></param>
        /// <returns></returns>
        /// 
        public override async Task<LocalPlayer> vmethod_2(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController)
        {
            spawnPoints = SpawnPoints.CreateFromScene(DateTime.Now, Location_0.SpawnPointParams);
            int spawnSafeDistance = Location_0.SpawnSafeDistanceMeters > 0 ? Location_0.SpawnSafeDistanceMeters : 100;
            SpawnSystemSettings settings = new(Location_0.MinDistToFreePoint, Location_0.MaxDistToFreePoint, Location_0.MaxBotPerZone, spawnSafeDistance);
            SpawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, () => Time.time, Singleton<GameWorld>.Instance, PBotsController, spawnPoints);
            spawnPoint = SpawnSystem.SelectSpawnPoint(ESpawnCategory.Player, Profile_0.Info.Side);

            if (spawnPoint == null)
            {
                Logger.LogError("SpawnPoint is Null");
                return null;
            }


            SendOrReceiveSpawnPoint(ref spawnPoint, spawnPoints);

            Logger.LogDebug($"{nameof(vmethod_2)}:Creating Owner CoopPlayer");
            var myPlayer = await CoopPlayer
               .Create(
               playerId
               , spawnPoint.Position
               , rotation
               , "Player"
               , ""
               , EPointOfView.FirstPerson
               , profile
               , aiControl: false
               , UpdateQueue
               , armsUpdateMode
               , EFT.Player.EUpdateMode.Auto
               , BackendConfigManager.Config.CharacterController.ClientPlayerMode
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
               , new FilterCustomizationClass()
               , null // Let the CoopPlayer Create handle this
               , null // Let the CoopPlayer Create handle this
               , isYourPlayer: true);
            // Inventory is FIR if Scav
            profile.SetSpawnedInSession(value: profile.Side == EPlayerSide.Savage);
            if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(CoopGameComponent)}");
                await Task.Delay(5000);
            }
            coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)myPlayer);
            coopGameComponent.ProfileIdsUser.Add(profile.Id);

            // Set Group Id for host
            myPlayer.Profile.Info.GroupId = "SIT";

            SendPlayerDataToServer(myPlayer);

            //SendOrReceiveSpawnPoint(myPlayer);

            // ---------------------------------------------
            // Here we can wait for other players, if desired
            TimeSpan waitTimeout = TimeSpan.FromSeconds(PluginConfigSettings.Instance.CoopSettings.WaitingTimeBeforeStart);

            await Task.Run(async () =>
            {
                if (coopGameComponent != null)
                {
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start the stopwatch immediately.

                    while (coopGameComponent.PlayerUsers == null)
                    {
                        Logger.LogDebug($"{nameof(vmethod_2)}: {nameof(coopGameComponent.PlayerUsers)} is null");
                        await Task.Delay(1000);
                    }

                    do
                    {
    
                        if (coopGameComponent.PlayerUsers == null || coopGameComponent.PlayerUsers.Count() == 0)
                        {
                            Logger.LogDebug($"{nameof(vmethod_2)}: PlayerUsers is null or empty");
                            await Task.Delay(1000);
                            continue;
                        }

                        var progress = coopGameComponent.PlayerUsers.Count() / SITMatchmaking.HostExpectedNumberOfPlayers;
                        var numbersOfPlayersToWaitFor = SITMatchmaking.HostExpectedNumberOfPlayers - coopGameComponent.PlayerUsers.Count();

                        if (SITMatchmaking.TimeHasComeScreenController != null)
                        {
                            SITMatchmaking.TimeHasComeScreenController.ChangeStatus(string.Format(StayInTarkovPlugin.LanguageDictionary["WAITING_PLAYER"].ToString(), numbersOfPlayersToWaitFor), progress);
                        }

                        if (coopGameComponent.PlayerUsers.Count() >= SITMatchmaking.HostExpectedNumberOfPlayers)
                        {
                            Logger.LogInfo("Desired number of players reached. Starting the game.");
                            break;
                        }

                        if (stopwatch.Elapsed >= waitTimeout)
                        {
                            Logger.LogInfo("Timeout reached. Proceeding with current players.");
                            break;
                        }

                        await Task.Delay(1000);

                    } while (true);

                    stopwatch.Stop();
                }
            });



            // ---------------------------------------------

            CoopPatches.EnableDisablePatches();

            return myPlayer;
            //return base.vmethod_2(playerId, position, rotation, layerName, prefix, pointOfView, profile, aiControl, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, statisticsManager, questController);
        }

        public static async void SendPlayerDataToServer(LocalPlayer player)
        {
            Logger.LogDebug($"{nameof(SendPlayerDataToServer)}");
            var profileJson = player.Profile.SITToJson();



            Dictionary<string, object> packet = new()
            {
                        {
                            "serverId",
                            SITMatchmaking.GetGroupId()
                        },
                        {
                        "isAI",
                            //player.IsAI && player.AIData != null && player.AIData.IsAI && !player.IsYourPlayer
                            !player.IsYourPlayer && (player as CoopPlayerClient == null)
                        },
                        {
                            "profileId",
                            player.ProfileId
                        },
                        {
                            "groupId",
                            SITMatchmaking.GetGroupId()
                        },
                        {
                            "sPx",
                            player.Transform.position.x
                        },
                        {
                            "sPy",
                            player.Transform.position.y
                        },
                        {
                            "sPz",
                            player.Transform.position.z
                        },
                        {
                            "profileJson",
                            profileJson
                        },
                        { "m", "PlayerSpawn" },
                    };


            //Logger.LogDebug(packet.ToJson());

            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            prc.player = player;
            //AkiBackendCommunicationCoop.PostLocalPlayerData(player, packet);

            // ------------------------------------------------------------------------------------
            // Send the information to the server
            Logger.LogDebug($"{nameof(SendPlayerDataToServer)}:PostJsonAsync");
            await AkiBackendCommunication.Instance.PostJsonAsync("/coop/server/update", packet.SITToJson());
        }

        /// <summary>
        /// Reconnection handling.
        /// </summary>
        public override void vmethod_3()
        {
            base.vmethod_3();
        }

        /// <summary>
        /// Bot System Starter -> Countdown
        /// </summary>
        /// <param name="startDelay"></param>
        /// <param name="controllerSettings"></param>
        /// <param name="spawnSystem"></param>
        /// <param name="runCallback"></param>
        /// <returns></returns>
        public override IEnumerator vmethod_4(float startDelay, BotControllerSettings controllerSettings, ISpawnSystem spawnSystem, Callback runCallback)
        {
            //Logger.LogDebug("vmethod_4");

            var shouldSpawnBots = !SITMatchmaking.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            if (!shouldSpawnBots)
            {
                controllerSettings.BotAmount = EBotAmount.NoBots;

                if (!PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem)
                    Logger.LogDebug("Bot Spawner System has been turned off - Wave System is Disabled");

                if (SITMatchmaking.IsSinglePlayer)
                    Logger.LogDebug("Bot Spawner System has been turned off - You are running as Single Player");

                if (SITMatchmaking.IsClient)
                    Logger.LogDebug("Bot Spawner System has been turned off - You are running as Client");
            }

            if (!SITMatchmaking.IsClient)
            {

                var nonwaves = (WaveInfo[])ReflectionHelpers.GetFieldFromTypeByFieldType(nonWavesSpawnScenario_0.GetType(), typeof(WaveInfo[])).GetValue(nonWavesSpawnScenario_0);

                BotsPresets profileCreator =
                    new(BackEndSession
                    , wavesSpawnScenario_0.SpawnWaves
                    , this.BossWaves
                    , nonwaves
                    , true);

                BotCreator botCreator = new(this, profileCreator, CreatePhysicalBot);
                BotZone[] botZones = LocationScene.GetAllObjects<BotZone>(false).ToArray();
                PBotsController.Init(this
                    , botCreator
                    , botZones
                    , spawnSystem
                    , wavesSpawnScenario_0.BotLocationModifier
                    , controllerSettings.IsEnabled && controllerSettings.BotAmount != EBotAmount.NoBots
                    , false // controllerSettings.IsScavWars
                    , true
                    , false // online
                    , GameDateTime.DateTime_0.Hour > 21 // have sectants
                    , Singleton<GameWorld>.Instance
                    , Location_0.OpenZones)
                    ;

                Logger.LogInfo($"Location: {Location_0.Name}");

                MaxBotCount = Location_0.BotMax != 0 ? Location_0.BotMax : controllerSettings.BotAmount switch
                {
                    EBotAmount.AsOnline => 20,
                    EBotAmount.Low => 15,
                    EBotAmount.Medium => 17,
                    EBotAmount.High => 19,
                    EBotAmount.Horde => 22,
                    _ => 22,
                };
                switch (controllerSettings.BotAmount)
                {
                    case EBotAmount.Low:
                        MaxBotCount = (int)Math.Floor(MaxBotCount * 0.9);
                        break;
                    case EBotAmount.High:
                        MaxBotCount = (int)Math.Floor(MaxBotCount * 1.1);
                        break;
                    case EBotAmount.Horde:
                        MaxBotCount = (int)Math.Floor(MaxBotCount * 1.25);
                        break;
                };

                int numberOfBots = shouldSpawnBots ? MaxBotCount : 0;
                Logger.LogDebug($"Max Number of Bots: {numberOfBots}");

                try
                {
                    PBotsController.SetSettings(numberOfBots, BackEndSession.BackEndConfig.BotPresets, BackEndSession.BackEndConfig.BotWeaponScatterings);
                    PBotsController.AddActivePLayer(PlayerOwner.Player);
                }
                catch (Exception ex)
                {
                    ConsoleScreen.LogException(ex);
                    Logger.LogError(ex);
                }
            }

            yield return new WaitForSeconds(startDelay);

            try
            {
                if (shouldSpawnBots)
                {
                    if (BossWaveManager != null)
                    {
                        Logger.LogDebug($"Running {nameof(BossWaveManager)}");
                        BossWaveManager.Run(EBotsSpawnMode.Anyway);
                    }

                    if (wavesSpawnScenario_0 != null && wavesSpawnScenario_0.SpawnWaves != null && wavesSpawnScenario_0.SpawnWaves.Length != 0)
                    {
                        Logger.LogDebug($"Running Wave Scenarios with Spawn Wave length : {wavesSpawnScenario_0.SpawnWaves.Length}");
                        wavesSpawnScenario_0.Run(EBotsSpawnMode.Anyway);
                    }

                }
                else
                {
                    if (wavesSpawnScenario_0 != null)
                        wavesSpawnScenario_0.Stop();
                    if (nonWavesSpawnScenario_0 != null)
                        nonWavesSpawnScenario_0.Stop();
                    if (BossWaveManager != null)
                        BossWaveManager.Stop();
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
                Logger.LogError(ex);
            }


            yield return new WaitForEndOfFrame();
            Logger.LogInfo("vmethod_4.SessionRun");

            // No longer need this ping. Load complete and all other data should keep happening after this point.
            StopCoroutine(ClientLoadingPinger());
            //GCHelpers.ClearGarbage(emptyTheSet: true, unloadAssets: false);

            var magazines = Profile_0.Inventory.AllRealPlayerItems.OfType<MagazineClass>().ToList();
            for (int i = 0; i < magazines.Count(); i++)
                Profile_0.CheckMagazines(magazines[i].Id, 2);

            // ------------------------------------------------------------------------
            // Setup Winter
            try
            {
                bool isWinter = BackEndSession.IsWinter;
                WinterEventController winterEventController = new WinterEventController();
                ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(GameWorld), typeof(WinterEventController)).SetValue(Singleton<GameWorld>.Instance, winterEventController);
                winterEventController.Run(isWinter).ContinueWith(x => { if (x.IsFaulted) Logger.LogError(x.Exception); return Task.CompletedTask; });
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
                Logger.LogError(ex);
            }

            if (shouldSpawnBots)
            {
                Logger.LogDebug($"Running Wave Scenarios");

                if (nonWavesSpawnScenario_0 != null)
                    nonWavesSpawnScenario_0.Run();

                if(wavesSpawnScenario_0 != null)
                    wavesSpawnScenario_0.Run();

            }

            yield return new WaitForEndOfFrame();

            // Add FreeCamController to GameWorld GameObject
            Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<FreeCameraController>();
            Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<SITAirdropsManager>();

            using (TokenStarter.StartWithToken("SessionRun"))
            {
                //vmethod_5();
                // below is vmethod_5
                CreateExfiltrationPointAndInitDeathHandler();
            }
            runCallback.Succeed();

        }

        public override void vmethod_5()
        {
            Logger.LogDebug(nameof(vmethod_5));
            //base.vmethod_5();
        }

        public void CreateExfiltrationPointAndInitDeathHandler()
        {
            try
            {
                Logger.LogInfo("CreateExfiltrationPointAndInitDeathHandler");

                GameTimer.Start();
                gparam_0.Player.HealthController.DiedEvent += HealthController_DiedEvent;
                gparam_0.vmethod_0();

                InfiltrationPoint = spawnPoint.Infiltration;
                Profile_0.Info.EntryPoint = InfiltrationPoint;

                //Logger.LogDebug(InfiltrationPoint);

                ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(Location_0.exits, justLoadSettings: false, "");
                ExfiltrationPoint[] exfilPoints = ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0);
                GameUi.TimerPanel.SetTime(DateTime.UtcNow, Profile_0.Info.Side, GameTimer.SessionSeconds(), exfilPoints);
                foreach (ExfiltrationPoint exfiltrationPoint in exfilPoints)
                {
                    exfiltrationPoint.OnStartExtraction += ExfiltrationPoint_OnStartExtraction;
                    exfiltrationPoint.OnCancelExtraction += ExfiltrationPoint_OnCancelExtraction;
                    exfiltrationPoint.OnStatusChanged += ExfiltrationPoint_OnStatusChanged;
                    UpdateExfiltrationUi(exfiltrationPoint, contains: false, initial: true);
                }

                // Paulov: You don't want this to run on Coop Game's
                //try
                //{
                //    EndByExitTrigerScenario.Run();
                //}
                //catch (Exception ex)
                //{
                //    ConsoleScreen.LogException(ex);
                //    Logger.LogError(ex);
                //}

                dateTime_0 = DateTime.Now;
                Status = GameStatus.Started;
                ConsoleScreen.ApplyStartCommands();
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
                Logger.LogError(ex);
            }
        }

        public Dictionary<string, (float, long, string)> ExtractingPlayers { get; } = new();
        public List<string> ExtractedPlayers { get; } = new();

        private void ExfiltrationPoint_OnCancelExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            if (!player.IsYourPlayer)
                return;

            Logger.LogDebug("ExfiltrationPoint_OnCancelExtraction");
            Logger.LogDebug(point.Status);

            ExtractingPlayers.Remove(player.ProfileId);

            MyExitLocation = null;
            //player.SwitchRenderer(true);
        }

        private void ExfiltrationPoint_OnStartExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            if (!player.IsYourPlayer)
                return;

            Logger.LogDebug("ExfiltrationPoint_OnStartExtraction");
            Logger.LogDebug(point.Settings.Name);
            Logger.LogDebug(point.Status);
            //Logger.LogInfo(point.ExfiltrationStartTime);
            Logger.LogDebug(point.Settings.ExfiltrationTime);
            bool playerHasMetRequirements = !point.UnmetRequirements(player).Any();
            //if (playerHasMetRequirements && !ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
            if (!ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
            {
                ExtractingPlayers.Add(player.ProfileId, (point.Settings.ExfiltrationTime, DateTime.Now.Ticks, point.Settings.Name));
                Logger.LogDebug($"Added {player.ProfileId} to {nameof(ExtractingPlayers)}");
            }
            //player.SwitchRenderer(false);

            MyExitLocation = point.Settings.Name;


        }

        private void ExfiltrationPoint_OnStatusChanged(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
        {
            UpdateExfiltrationUi(point, point.Entered.Any((x) => x.ProfileId == Profile_0.Id));
            Logger.LogDebug("ExfiltrationPoint_OnStatusChanged");
            EExfiltrationStatus curStatus = point.Status;
            Logger.LogDebug($"{nameof(prevStatus)}:{prevStatus}.{nameof(curStatus)}:{curStatus}");

            // Fixes player cannot extract with The Lab elevator and Armored Train
            if (prevStatus == EExfiltrationStatus.AwaitsManualActivation && curStatus == EExfiltrationStatus.Countdown)
                point.ExternalSetStatus(EExfiltrationStatus.RegularMode);

            // Fixes player cannot extract with Car on Ground Zero
            if (prevStatus == EExfiltrationStatus.UncompleteRequirements && curStatus == EExfiltrationStatus.Countdown)
            {
                foreach (var player in point.Entered)
                {
                    if (!ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
                    {
                        ExtractingPlayers.Add(player.ProfileId, (point.Settings.ExfiltrationTime, DateTime.Now.Ticks, point.Settings.Name));
                        Logger.LogDebug($"Added {player.ProfileId} to {nameof(ExtractingPlayers)}");
                    }
                }
                point.ExternalSetStatus(EExfiltrationStatus.RegularMode);
            }
        }

        public ExitStatus MyExitStatus { get; set; } = ExitStatus.Survived;
        public string MyExitLocation { get; set; } = null;
        public ISpawnSystem SpawnSystem { get; set; }
        public int MaxBotCount { get; private set; }
        public IGameClient GameClient { get; private set; }
        public GameServerUDP GameServer { get; private set; }

        private void HealthController_DiedEvent(EDamageType obj)
        {
            //Logger.LogInfo(ScreenManager.Instance.CurrentScreenController.ScreenType);

            //Logger.LogInfo("CoopGame.HealthController_DiedEvent");


            gparam_0.Player.HealthController.DiedEvent -= method_15;
            gparam_0.Player.HealthController.DiedEvent -= HealthController_DiedEvent;

            PlayerOwner.vmethod_1();
            MyExitStatus = ExitStatus.Killed;
            MyExitLocation = null;

        }

        public override void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
        {
            //Status = GameStatus.Stopped;

            Logger.LogInfo("CoopGame.Stop");

            // If I am the Host/Server, then ensure all the bots have left too
            if (SITMatchmaking.IsServer)
            {
                foreach (var p in CoopGameComponent.GetCoopGameComponent().Players)
                {
                    AkiBackendCommunication.Instance.PostJson("/coop/server/update", new Dictionary<string, object>() {

                            { "m", "PlayerLeft" },
                            { "profileId", p.Value.ProfileId },
                            { "serverId", CoopGameComponent.GetServerId() }

                        }.ToJson());
                }
            }

            // Notify that I have left the Server
            AkiBackendCommunication.Instance.PostJson("/coop/server/update", new Dictionary<string, object>() {
                { "m", "PlayerLeft" },
                { "profileId", Singleton<GameWorld>.Instance.MainPlayer.ProfileId },
                { "serverId", CoopGameComponent.GetServerId() }

            }.ToJson());

            


            if (BossWaveManager != null)
                BossWaveManager.Stop();

            if (nonWavesSpawnScenario_0 != null)
                nonWavesSpawnScenario_0.Stop();

            if (wavesSpawnScenario_0 != null)
                wavesSpawnScenario_0.Stop();


            CoopPatches.EnableDisablePatches();
            //base.Stop(profileId, exitStatus, exitName, delay);

            // -----------------------------------------------------------------------------------------------
            // Paulov: This is BaseLocalGame Stop method
            if (profileId != Profile_0.Id || base.Status == GameStatus.Stopped || base.Status == GameStatus.Stopping)
            {
                return;
            }
            if (base.Status == GameStatus.Starting || base.Status == GameStatus.Started)
            {
                ReflectionHelpers.GetFieldFromType(EndByTimerScenario.GetType(), "GameStatus_0").SetValue(EndByTimerScenario, GameStatus.SoftStopping);
            }
            base.Status = GameStatus.Stopping;
            base.GameTimer.TryStop();
            EndByExitTrigerScenario.Stop();
            GameUi.TimerPanel.Close();

            try
            {
                if (!SITMatchmaking.IsClient)
                {
                    botsController_0.Stop();
                    botsController_0.DestroyInfo(gparam_0.Player);
                }
            }
            catch (Exception)
            {

            }

            if (EnvironmentManager.Instance != null)
            {
                EnvironmentManager.Instance.Stop();
            }
            MonoBehaviourSingleton<PreloaderUI>.Instance.StartBlackScreenShow(1f, 1f, delegate
            {
                ScreenManager instance = ScreenManager.Instance;
                if (instance.CheckCurrentScreen(EEftScreenType.Reconnect))
                {
                    instance.CloseAllScreensForced();
                }
                gparam_0.Player.OnGameSessionEnd(exitStatus, base.PastTime, Location_0.Id, exitName);
                CleanUp();
                base.Status = GameStatus.Stopped;
                TimeSpan timeSpan = DateTime.Now - dateTime_0;
                _ = BackEndSession.OfflineRaidEnded(exitStatus, exitName, timeSpan.TotalSeconds);
                MonoBehaviourSingleton<BetterAudio>.Instance.FadeOutVolumeAfterRaid();
                StaticManager.Instance.WaitSeconds(delay, delegate
                {
                    Callback<ExitStatus, TimeSpan, MetricsClass> callback = ReflectionHelpers.GetFieldFromType(this.GetType(), "callback_0").GetValue(this) as Callback<ExitStatus, TimeSpan, MetricsClass>;
                    callback(new Result<ExitStatus, TimeSpan, MetricsClass>(exitStatus, DateTime.Now - dateTime_0, new MetricsClass()));
                    UIEventSystem.Instance.Enable();
                });
            });
            // end of BaseLocalGame Stop method
            // -----------------------------------------------------------------------------------------------

            CoopPatches.LeftGameDestroyEverything();
        }

        public override void CleanUp()
        {
            base.CleanUp();
            smethod_4(Bots);
        }

        public override void Dispose()
        {
            Logger.LogDebug("CoopGame:Dispose()");
            StartCoroutine(DisposingCo());
            base.Dispose();
        }

        private IEnumerator DisposingCo()
        {
            Logger.LogDebug("CoopGame:DisposingCo()");
            CoopPatches.LeftGameDestroyEverything();

            yield break;
        }

        private BossWaveManager BossWaveManager;

        private WavesSpawnScenario wavesSpawnScenario_0;

        public BossLocationSpawn[] BossWaves { get; private set; }

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

        private Func<EFT.Player, GamePlayerOwner> func_1;


        public new void method_6(string backendUrl, string locationId, int variantId)
        {
            Logger.LogInfo("CoopGame:method_6");
            return;
        }
    }
}
