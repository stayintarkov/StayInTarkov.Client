/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */


using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.AssetsManager;
using EFT.Bots;
using EFT.CameraControl;
using EFT.Counters;
using EFT.EnvironmentEffect;
using EFT.Game.Spawning;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.MovingPlatforms;
using EFT.UI;
using EFT.UI.Matchmaker;
using EFT.UI.Screens;
using EFT.Weather;
using JsonType;
using Newtonsoft.Json.Linq;
using StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.FreeCamera;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Raid;
using StayInTarkov.Coop.NetworkPacket.World;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace StayInTarkov.Coop.SITGameModes
{
    /// <summary>
    /// A custom Game Type
    /// </summary>
    public sealed class CoopSITGame : BaseLocalGame<EftGamePlayerOwner>, IBotGame, ISITGame
    {
        public string DisplayName { get; } = "Coop Game";

        public new bool InRaid { get { return true; } }

        public string InfiltrationPoint { get; set; }

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


        private static ManualLogSource Logger;

        public DateTime GameWorldTime { get; set; }

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
            Logger.LogInfo("CoopGame.Create");

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
            WildSpawnWave[] waves = FixScavWaveSettings(wavesSettings, location.waves);
            coopGame.nonWavesSpawnScenario_0 = NonWavesSpawnScenario.smethod_0(coopGame, location, coopGame.PBotsController);
            coopGame.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            // ---------------------------------------------------------------------------------
            // Waves Scenario setup
            coopGame.wavesSpawnScenario_0 = WavesSpawnScenario.smethod_0(
                    coopGame.gameObject
                    , waves
                    , new Action<BotSpawnWave>((wave) => coopGame.PBotsController.ActivateBotsByWave(wave))
                    , location);

            // ---------------------------------------------------------------------------------
            // Setup Boss Wave Manager
            coopGame.BossWaves = coopGame.FixBossWaveSettings(wavesSettings, location);
            var bosswavemanagerValue = BossWaveManager.smethod_0(coopGame.BossWaves, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }));
            coopGame.BossWaveManager = bosswavemanagerValue;
            coopGame.func_1 = delegate (EFT.Player player)
            {
                var val = EftGamePlayerOwner.Create<EftGamePlayerOwner>(player, inputTree, insurance, backEndSession, gameUI, coopGame.GameDateTime, location);
                val.OnLeave += coopGame.vmethod_3;
                return val;
            };

            // ---------------------------------------------------------------------------------
            // Setup ISITGame Singleton
            Singleton<ISITGame>.Create(coopGame);

            // ---------------------------------------------------------------------------------
            // Create Coop Game Component
            Logger.LogDebug($"{nameof(Create)}:Running {nameof(coopGame.CreateCoopGameComponent)}");
            coopGame.CreateCoopGameComponent();

            // ---------------------------------------------------------------------------------
            // Create GameClient(s)
            switch (SITMatchmaking.SITProtocol)
            {
                case ESITProtocol.RelayTcp:
                    coopGame.GameClient = coopGame.GetOrAddComponent<GameClientTCPRelay>();

                    break;
                case ESITProtocol.PeerToPeerUdp:
                    if (SITMatchmaking.IsServer)
                        coopGame.GameServer = coopGame.GetOrAddComponent<GameServerUDP>();

                    coopGame.GameClient = coopGame.GetOrAddComponent<GameClientUDP>();
                    break;
                default:
                    throw new Exception("Unknown SIT Protocol used!");

            }

            return coopGame;
        }

        public void CreateCoopGameComponent()
        {
            //var coopGameComponent = SITGameComponent.GetCoopGameComponent();
            //if (coopGameComponent != null)
            //{
            //    Destroy(coopGameComponent);
            //}

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
            var coopGameComponent = CoopPatches.CoopGameComponentParent.AddComponent<SITGameComponent>();

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
                StartCoroutine(GameTimerSync());
                StartCoroutine(ArmoredTrainTimeSync());
            }

            clientLoadingPingerCoroutine = StartCoroutine(ClientLoadingPinger());

            var friendlyAIJson = AkiBackendCommunication.Instance.GetJson($"/coop/server/friendlyAI/{SITGameComponent.GetServerId()}");
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
                AkiBackendCommunication.Instance?.PingAsync();

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

        private IEnumerator GameTimerSync()
        {
            var waitSeconds = new WaitForSeconds(10f);

            while (true)
            {
                yield return waitSeconds;

                if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    yield break;

                if (GameTimer.StartDateTime.HasValue && GameTimer.SessionTime.HasValue)
                {
                    RaidTimerPacket packet = new RaidTimerPacket();
                    packet.SessionTime = (GameTimer.SessionTime - GameTimer.PastTime).Value.Ticks;
                    Networking.GameClient.SendData(packet.Serialize());
                }
            }
        }

        private IEnumerator ArmoredTrainTimeSync()
        {
            var waitSeconds = new WaitForSeconds(30f);

            while (true)
            {
                yield return waitSeconds;

                if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
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


        public static WildSpawnWave[] FixScavWaveSettings(WavesSettings wavesSettings, WildSpawnWave[] waves)
        {
            Logger.LogDebug($"{nameof(CoopSITGame)}:{nameof(FixScavWaveSettings)}");

            foreach (WildSpawnWave wildSpawnWave in waves)
            {
                wildSpawnWave.slots_min = wavesSettings.BotAmount == EBotAmount.NoBots ? 0 : 1;
                wildSpawnWave.slots_max = wavesSettings.BotAmount == EBotAmount.NoBots ? 0 : Math.Max(1, wildSpawnWave.slots_max);
                if (wavesSettings.IsTaggedAndCursed && wildSpawnWave.WildSpawnType == WildSpawnType.assault)
                {
                    wildSpawnWave.WildSpawnType = WildSpawnType.cursedAssault;
                }
                if (wavesSettings.IsBosses)
                {
                    wildSpawnWave.time_min += 5;
                    wildSpawnWave.time_max += 27;
                }
                wildSpawnWave.BotDifficulty = ToBotDifficulty(wavesSettings.BotDifficulty);
            }
            return waves;
        }

        public static BotDifficulty ToBotDifficulty(EBotDifficulty botDifficulty)
        {
            return botDifficulty switch
            {
                EBotDifficulty.Easy => BotDifficulty.easy,
                EBotDifficulty.Medium => BotDifficulty.normal,
                EBotDifficulty.Hard => BotDifficulty.hard,
                EBotDifficulty.Impossible => BotDifficulty.impossible,
                EBotDifficulty.Random => SelectRandomBotDifficulty(),
                _ => BotDifficulty.normal,
            };
        }

        public static BotDifficulty SelectRandomBotDifficulty()
        {
            Array values = Enum.GetValues(typeof(BotDifficulty));
            return (BotDifficulty)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        private static int[] CultistSpawnTime = new[] { 22, 6 };

        private static bool CanSpawnCultist(int hour)
        {
            if (hour >= CultistSpawnTime[0] && hour <= CultistSpawnTime[1])
                return true;

            return false;
        }

        public BossLocationSpawn[] FixBossWaveSettings(WavesSettings wavesSettings, LocationSettingsClass.Location location)
        {
            var bossLocationSpawns = location.BossLocationSpawn;
            TimeSpan CurrentGameTime = GameDateTime.Calculate().TimeOfDay;
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

                if (!CanSpawnCultist(CurrentGameTime.Hours) && bossLocationSpawn.BossName.Contains("sectant"))
                {
                    Logger.LogInfo($"Block spawn of Sectant (Cultist) in day time in hour {CurrentGameTime.Hours}!");
                    bossLocationSpawn.BossChance = 0f;
                }

                //ArchangelWTF: boss types like 'arenaFighterEvent' can have multiple values, split these out and take the first value.
                //We could maybe do some fancy randomization here at some point to get a number in between the two values, but for now this works.
                if (bossLocationSpawn.BossEscortAmount.Contains(","))
                    bossLocationSpawn.BossEscortAmount = bossLocationSpawn.BossEscortAmount.Split(',')[0];

                int EscortAmount = Convert.ToInt32(bossLocationSpawn.BossEscortAmount);

                if (bossLocationSpawn.Supports == null && !string.IsNullOrEmpty(bossLocationSpawn.BossEscortType) && EscortAmount > 0)
                {
                    Logger.LogDebug($"bossLocationSpawn.Supports is Null. Attempt to create them.");

                    Enum.TryParse<WildSpawnType>(bossLocationSpawn.BossEscortType, out var EscortType);

                    bossLocationSpawn.Supports = new WildSpawnSupports[EscortAmount];

                    for (int i = 0; i < EscortAmount; i++)
                    {
                        bossLocationSpawn.Supports[i] = new WildSpawnSupports
                        {
                            BossEscortDifficult = new[] { bossLocationSpawn.BossEscortDifficult },
                            BossEscortAmount = 1,
                            BossEscortType = EscortType
                        };
                    }
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

            if (Bots != null && !profile.Info.Settings.IsBossOrFollower() && Bots.Count(x => x.Value != null && x.Value.PlayerHealthController.IsAlive) >= MaxBotCount)
            {
                Logger.LogDebug("Block spawn of Bot. Max Bot Count has been reached!");
                return null;
            }

            if (!CanSpawnCultist(GameDateTime.Calculate().TimeOfDay.Hours) && profile.Info != null && profile.Info.Settings != null
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

                // Paulov: After 0.14.5.5 release, I had to add these lines otherwise bundles would not be loaded before the bot is created
                var allPrefabPaths = profile.GetAllPrefabPaths();
                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, allPrefabPaths.ToArray(), JobPriority.General);

                // Create the Bot
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

                if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                {
                    Logger.LogDebug($"{nameof(CreatePhysicalBot)}:Unable to find {nameof(SITGameComponent)}");
                    await Task.Delay(5000);
                }

                // 0.14 update. Add to ProfileIdsAI list.
                // Add to CoopGameComponent list
                coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)botPlayer);
                coopGameComponent.ProfileIdsAI.Add(profile.Id);

                SendPlayerDataToServer(botPlayer, position, true);

            }
            return botPlayer;
        }

        /// <summary>
        /// Matchmaker countdown
        /// </summary>
        public override IEnumerator vmethod_1()
        {
            int timeBeforeDeployLocal = Singleton<BackendConfigSettingsClass>.Instance.TimeBeforeDeployLocal;
            DateTime gameStartTime = DateTime.Now.AddSeconds(timeBeforeDeployLocal);
            new MatchmakerFinalCountdown.GClass3180(base.Profile_0, gameStartTime).ShowScreen(EScreenState.Root);
            MonoBehaviourSingleton<BetterAudio>.Instance.FadeInVolumeBeforeRaid(timeBeforeDeployLocal);
            Singleton<GUISounds>.Instance.StopMenuBackgroundMusicWithDelay(timeBeforeDeployLocal);
            GameUi.gameObject.SetActive(value: true);
            GameUi.TimerPanel.ProfileId = ProfileId;
            yield return new WaitForSeconds(timeBeforeDeployLocal);
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
                        SITGameComponent.GetServerId()
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
          
            }
            else if (SITMatchmaking.IsClient)
            {
                if (PluginConfigSettings.Instance.CoopSettings.AllPlayersSpawnTogether)
                {
                    var json = AkiBackendCommunication.Instance.GetJson($"/coop/server/spawnPoint/{SITGameComponent.GetServerId()}");
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
        /// 
        public override async Task<LocalPlayer> vmethod_2(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController)
        {
            // Send Connect Command to Relay
            switch (SITMatchmaking.SITProtocol)
            {
                case ESITProtocol.RelayTcp:
                    JObject j = new JObject();
                    j.Add("serverId", SITGameComponent.GetServerId());
                    j.Add("profileId", profile.ProfileId);
                    j.Add("connect", true);
                    Logger.LogDebug("Sending Connect to Relay");
                    GameClient.SendData(Encoding.UTF8.GetBytes(j.ToString()));
                    break;
            }



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
               , FilterCustomizationClass.Default
               , isYourPlayer: true);
            // Inventory is FIR if Scav
            profile.SetSpawnedInSession(value: profile.Side == EPlayerSide.Savage);
            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(SITGameComponent)}");
                await Task.Delay(5000);
            }
            coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)myPlayer);
            coopGameComponent.ProfileIdsUser.Add(profile.Id);

            // Set Group Id for host
            myPlayer.Profile.Info.GroupId = "SIT";
            myPlayer.Transform.position = spawnPoint.Position;
            SendPlayerDataToServer(myPlayer, spawnPoint.Position, false);

            //SendOrReceiveSpawnPoint(myPlayer);


            //WaitForPlayers


            // ---------------------------------------------

            CoopPatches.EnableDisablePatches();

            return myPlayer;
        }

        private async Task WaitForPlayersToSpawn()
        {
            if (SITMatchmaking.TimeHasComeScreenController != null)
            {
                SITMatchmaking.TimeHasComeScreenController.ChangeStatus($"Session Started. Waiting for Player(s)");
                await Task.Delay(1000);
            }

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(SITGameComponent)}");
                await Task.Delay(5000);
            }

            // ---------------------------------------------
            // Here we can wait for other players, if desired
            TimeSpan waitTimeout = TimeSpan.FromSeconds(PluginConfigSettings.Instance.CoopSettings.WaitingTimeBeforeStart);

            //await Task.Run(async () =>
            //{
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

                    await Task.Delay(1000);

                    if (coopGameComponent.PlayerUsers == null || coopGameComponent.PlayerUsers.Count() == 0)
                    {
                        Logger.LogDebug($"{nameof(vmethod_2)}: PlayerUsers is null or empty");
                        await Task.Delay(1000);
                        continue;
                    }

                    //await Task.Run(() =>
                    //{
                    // Ensure this is a distinct list of Ids
                    //var distinctExistingProfileIds = playerList.Distinct().ToArray();
                    SendRequestSpawnPlayersPacket();
                    //});

                    var numbersOfPlayersToWaitFor = SITMatchmaking.HostExpectedNumberOfPlayers - coopGameComponent.PlayerUsers.Count();

                    if (SITMatchmaking.TimeHasComeScreenController != null)
                    {
                        SITMatchmaking.TimeHasComeScreenController.ChangeStatus(string.Format(StayInTarkovPlugin.LanguageDictionary["WAITING_FOR_PLAYERS_TO_SPAWN"].ToString(), numbersOfPlayersToWaitFor));
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

                } while (true);

                stopwatch.Stop();
            }
            //});

            ReadyToStartGamePacket packet = new ReadyToStartGamePacket(SITMatchmaking.Profile.ProfileId);
            GameClient.SendData(packet.Serialize());
        }

        private async Task WaitForPlayersToBeReady()
        {
            if (SITMatchmaking.TimeHasComeScreenController != null)
            {
                SITMatchmaking.TimeHasComeScreenController.ChangeStatus($"Players spawned. Waiting for Player(s) to be Ready.");
                await Task.Delay(1000);
            }

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(SITGameComponent)}");
                await Task.Delay(5000);
            }

            // ---------------------------------------------
            // Here we can wait for other players, if desired
            TimeSpan waitTimeout = TimeSpan.FromSeconds(PluginConfigSettings.Instance.CoopSettings.WaitingTimeBeforeStart);

            //await Task.Run(async () =>
            //{
            if (coopGameComponent != null)
            {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start the stopwatch immediately.



                do
                {

                    await Task.Delay(1000);

                    var numbersOfPlayersToWaitFor = SITMatchmaking.HostExpectedNumberOfPlayers - ReadyPlayers;

                    if (SITMatchmaking.TimeHasComeScreenController != null)
                    {
                        SITMatchmaking.TimeHasComeScreenController.ChangeStatus(string.Format(StayInTarkovPlugin.LanguageDictionary["WAITING_FOR_PLAYERS_TO_BE_READY"].ToString(), numbersOfPlayersToWaitFor));
                    }

                    if (ReadyPlayers >= SITMatchmaking.HostExpectedNumberOfPlayers)
                    {
                        Logger.LogInfo("Desired number of players reached. Starting the game.");
                        break;
                    }

                    if (stopwatch.Elapsed >= waitTimeout)
                    {
                        Logger.LogInfo("Timeout reached. Proceeding with current players.");
                        break;
                    }

                } while (true);

                stopwatch.Stop();
            }
            //});

            if (!SITMatchmaking.IsClient)
            {
                HostStartingGamePacket packet = new HostStartingGamePacket();
                GameClient.SendData(packet.Serialize());
            }
        }

        private async Task WaitForHostToStart()
        {
            if (SITMatchmaking.TimeHasComeScreenController != null)
            {
                SITMatchmaking.TimeHasComeScreenController.ChangeStatus($"Players spawned and ready. Waiting for Host to start.");
                await Task.Delay(1000);
            }

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(SITGameComponent)}");
                await Task.Delay(5000);
            }

            // ---------------------------------------------
            // Here we can wait for other players, if desired
            TimeSpan waitTimeout = TimeSpan.FromSeconds(PluginConfigSettings.Instance.CoopSettings.WaitingTimeBeforeStart);

            if (coopGameComponent != null)
            {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start the stopwatch immediately.



                do
                {

                    await Task.Delay(1000);

                    if (SITMatchmaking.TimeHasComeScreenController != null)
                    {
                        SITMatchmaking.TimeHasComeScreenController.ChangeStatus(StayInTarkovPlugin.LanguageDictionary["WAITING_FOR_HOST_TO_BE_READY"].ToString());
                    }

                    if (HostReady)
                        break;

                    if (stopwatch.Elapsed >= waitTimeout)
                    {
                        Logger.LogInfo("Timeout reached. Proceeding with current players.");
                        break;
                    }

                } while (true);

                stopwatch.Stop();
            }
        }

        private void SendRequestSpawnPlayersPacket()
        {
            RequestSpawnPlayersPacket requestSpawnPlayersPacket = new RequestSpawnPlayersPacket([Singleton<GameWorld>.Instance.MainPlayer.ProfileId]);
            GameClient.SendData(requestSpawnPlayersPacket.Serialize());
        }

        public static void SendPlayerDataToServer(LocalPlayer player, Vector3 position, bool isAI)
        {
#if DEBUG
            Logger.LogDebug($"{nameof(SendPlayerDataToServer)}");
#endif

            // Sends out to all clients that this Character has spawned
            var infoPacket = SpawnPlayersPacket.CreateInformationPacketFromPlayer(player);
            infoPacket.BodyPosition = position;
            infoPacket.IsAI = isAI;
            var spawnPlayersPacket = new SpawnPlayersPacket([infoPacket]);
            Networking.GameClient.SendData(spawnPlayersPacket.Serialize());
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
        public override IEnumerator vmethod_4(BotControllerSettings controllerSettings, ISpawnSystem spawnSystem, Callback runCallback)
        {
            //Logger.LogDebug("vmethod_4");

            yield return StartCoroutine(vmethod_1());

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
            StopCoroutine(clientLoadingPingerCoroutine);

            var magazines = Profile_0.Inventory.AllRealPlayerItems.OfType<MagazineClass>().ToList();
            for (int i = 0; i < magazines.Count(); i++)
                Profile_0.CheckMagazines(magazines[i].Id, 2);

            if (shouldSpawnBots)
            {
                Logger.LogDebug($"Running Wave Scenarios");

                if (nonWavesSpawnScenario_0 != null)
                    nonWavesSpawnScenario_0.Run();

                if (wavesSpawnScenario_0 != null)
                    wavesSpawnScenario_0.Run();

            }

            yield return new WaitForEndOfFrame();

            // Paulov: 0.14.5.5. TODO: This is the Events / Seasonal stuff. We need some data and test this out.
            //BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
            //if (instance != null && instance.EventSettings.EventActive && !instance.EventSettings.LocationsToIgnore.Contains(base.Location_0.Id))
            //{
            //    Singleton<GameWorld>.Instance.HalloweenEventController = new HalloweenEventControllerClass();
            //    GameObject gameObject = (GameObject)Resources.Load("Prefabs/HALLOWEEN_CONTROLLER");
            //    if (gameObject != null)
            //    {
            //        GClass5.InstantiatePrefab(base.transform, gameObject);
            //    }
            //    else
            //    {
            //        UnityEngine.Debug.LogError("Can't find event prefab in resources. Path : Prefabs/HALLOWEEN_CONTROLLER");
            //    }
            //}
            //ESeason season = BackEndSession.Season;
            //Class392 @class = new Class392();
            //Singleton<GameWorld>.Instance.GInterface26_0 = @class;
            //Task task = @class.Run(season);
            //yield return new WaitUntil(() => task.IsCompleted);

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

                ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(Location_0.exits, justLoadSettings: SITMatchmaking.IsClient, "");
                ExfiltrationPoint[] exfilPoints = ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0);
                GameUi.TimerPanel.SetTime(DateTime.UtcNow, Profile_0.Info.Side, GameTimer.SessionSeconds(), exfilPoints);
                foreach (ExfiltrationPoint exfiltrationPoint in exfilPoints)
                {
                    exfiltrationPoint.OnStartExtraction += ExfiltrationPoint_OnStartExtraction;
                    exfiltrationPoint.OnCancelExtraction += ExfiltrationPoint_OnCancelExtraction;
                    exfiltrationPoint.OnStatusChanged += ExfiltrationPoint_OnStatusChanged;
                    UpdateExfiltrationUi(exfiltrationPoint, contains: false, initial: true);
                }

                // Paulov: You don't want this to run on Coop Game's as the raid will end when the host extracts
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

                ConsoleScreen.Processor.RegisterCommandGroup<StayInTarkov.UI.ConsoleCommands>();
                ConsoleScreen.ApplyStartCommands();
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
                Logger.LogError(ex);
            }
        }

        public List<ExfiltrationPoint> EnabledCountdownExfils { get; private set; } = new();
        public Dictionary<string, (float, long, string)> ExtractingPlayers { get; } = new();
        public List<string> ExtractedPlayers { get; } = new();

        private void ExfiltrationPoint_OnCancelExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            if (!player.IsYourPlayer)
                return;

            Logger.LogDebug($"{nameof(ExfiltrationPoint_OnCancelExtraction)} {point.Settings.Name} {point.Status}");
            ExtractingPlayers.Remove(player.ProfileId);


            var matchEnd = Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd;


            if (Profile_0.EftStats.SessionCounters.GetAllInt(new object[] { CounterTag.Exp }) > matchEnd.SurvivedExpRequirement ||
                RaidTimeUtil.GetElapsedRaidSeconds() > matchEnd.SurvivedTimeRequirement)
            {
                MyExitStatus = (player.HealthController.IsAlive ? ExitStatus.MissingInAction : ExitStatus.Killed);
            }
            else
            {
                MyExitStatus = ExitStatus.Runner;
            }

            MyExitLocation = null;
        }

        private void ExfiltrationPoint_OnStartExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            if (!player.IsYourPlayer)
                return;

            Logger.LogDebug($"{nameof(ExfiltrationPoint_OnStartExtraction)} {point.Settings.Name} {point.Status} {point.Settings.ExfiltrationTime}");
            bool playerHasMetRequirements = !point.UnmetRequirements(player).Any();
            if (!ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
            {
                ExtractingPlayers.Add(player.ProfileId, (point.Settings.ExfiltrationTime, DateTime.Now.Ticks, point.Settings.Name));
                Logger.LogDebug($"Added {player.ProfileId} to {nameof(ExtractingPlayers)}");
            }

            MyExitLocation = point.Settings.Name;
            MyExitStatus = ExitStatus.Survived;
        }

        private void ExfiltrationPoint_OnStatusChanged(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
        {
            UpdateExfiltrationUi(point, point.Entered.Any((x) => x.ProfileId == Profile_0.Id));
            EExfiltrationStatus curStatus = point.Status;
            Logger.LogDebug($"{nameof(ExfiltrationPoint_OnStatusChanged)} {nameof(prevStatus)}={prevStatus} {nameof(curStatus)}={curStatus}");

            // Manage countdown exfils
            var countingDown = EnabledCountdownExfils.Contains(point);
            if (!countingDown && point.Status == EExfiltrationStatus.Countdown)
            {
                if (point.ExfiltrationStartTime <= 0f)
                {
                    point.ExfiltrationStartTime = this.PastTime;
                }
                EnabledCountdownExfils.Add(point);
            }
            else if (countingDown && point.Status != EExfiltrationStatus.Countdown)
            {
                point.ExfiltrationStartTime = -float.Epsilon;
                EnabledCountdownExfils.Remove(point);
            }

            // Paulov: Had to add this. Without a SITGameComponent, the ServerId is null when sending data. Therefore a Raid could fail and blank screen.
            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                // Propagate exfil point state to all players (useful for Countdown exfils, like car)
                // We do not propagate NotPresent because clients are responsible to trigger their local exfils.
                // A race condition would cause NotPresent to be received before clients can properly process the exfil logic
                // because EFT's code clears ExfiltrationPoint.Entered upon setting NotPresent
                if (prevStatus != curStatus && curStatus != EExfiltrationStatus.NotPresent && curStatus != EExfiltrationStatus.UncompleteRequirements)
                {
                    UpdateExfiltrationPointPacket packet = new()
                    {
                        PointName = point.Settings.Name,
                        Command = curStatus,
                        QueuedPlayers = point.QueuedPlayers
                    };
                    GameClient.SendData(packet.Serialize());
                }
            }
        }

        public ExitStatus MyExitStatus { get; set; } = ExitStatus.MissingInAction;
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
                foreach (var p in SITGameComponent.GetCoopGameComponent().Players)
                {
                    AkiBackendCommunication.Instance.PostJson("/coop/server/update", new Dictionary<string, object>() {

                            { "m", "PlayerLeft" },
                            { "profileId", p.Value.ProfileId },
                            { "serverId", SITGameComponent.GetServerId() }

                        }.ToJson());
                }
            }

            // Notify that I have left the Server
            AkiBackendCommunication.Instance.PostJson("/coop/server/update", new Dictionary<string, object>() {
                { "m", "PlayerLeft" },
                { "profileId", Singleton<GameWorld>.Instance.MainPlayer.ProfileId },
                { "serverId", SITGameComponent.GetServerId() }

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
            if (!SITMatchmaking.IsClient)
            {
                botsController_0.Stop();
                botsController_0.DestroyInfo(gparam_0.Player);
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

            //CoopPatches.LeftGameDestroyEverything();
        }

        //public new void Update()
        //{
        //    UpdateByUnity?.Invoke();    
        //}

        public override void CleanUp()
        {
            foreach (EFT.Player value in Bots.Values)
            {
                try
                {
                    value.Dispose();
                    AssetPoolObject.ReturnToPool(value.gameObject);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
            Bots.Clear();

            if (SITGameComponent.TryGetCoopGameComponent(out var gameComponent))
            {
                if (gameComponent.PlayerClients != null)
                {
                    foreach (EFT.Player value in gameComponent.PlayerClients)
                    {
                        try
                        {
                            value.Dispose();
                            AssetPoolObject.ReturnToPool(value.gameObject);
                        }
                        catch (Exception exception)
                        {
                            UnityEngine.Debug.LogException(exception);
                        }
                    }
                    gameComponent.PlayerClients.Clear();
                }
            }

            base.CleanUp();
        }

        public override void Dispose()
        {
            Logger.LogDebug("CoopGame:Dispose()");
            StartCoroutine(DisposingCo());

            foreach (ExfiltrationPoint exfiltrationPoint in ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0))
            {
                exfiltrationPoint.OnStartExtraction -= ExfiltrationPoint_OnStartExtraction;
                exfiltrationPoint.OnCancelExtraction -= ExfiltrationPoint_OnCancelExtraction;
                exfiltrationPoint.OnStatusChanged -= ExfiltrationPoint_OnStatusChanged;
            }

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
        public int ReadyPlayers { get; set; }
        public bool HostReady { get; set; }

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

        private Func<EFT.Player, EftGamePlayerOwner> func_1;
        private Coroutine clientLoadingPingerCoroutine;

        public new void method_6(string backendUrl, string locationId, int variantId)
        {
            Logger.LogInfo("CoopGame:method_6");
            return;
        }

        public async Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryControllerClass inventoryController, Callback runCallback)
        {
            Logger.LogDebug(nameof(Run));

            base.Status = GameStatus.Running;
            UnityEngine.Random.InitState((int)DateTime.UtcNow.Ticks);
            LocationSettingsClass.Location location;
            if (Location_0.IsHideout)
            {
                location = Location_0;
            }
            else
            {
                using (TokenStarter.StartWithToken("LoadLocation"))
                {
                    int variantId = UnityEngine.Random.Range(1, 6);
                    method_6(backendUrl, Location_0.Id, variantId);
                    location = await BackEndSession.LoadLocationLoot(Location_0.Id, variantId);
                }
            }
            BackendConfigManagerConfig config = BackendConfigManager.Config;
            if (config.FixedFrameRate > 0f)
            {
                base.FixedDeltaTime = 1f / config.FixedFrameRate;
            }
            EFT.Player player = await CreatePlayerSpawn();
            dictionary_0.Add(player.ProfileId, player);
            gparam_0 = func_1(player);
            PlayerCameraController.Create(gparam_0.Player);
            FPSCamera.Instance.SetOcclusionCullingEnabled(Location_0.OcculsionCullingEnabled);
            FPSCamera.Instance.IsActive = false;

            await SpawnLoot(location);
            await WaitForPlayersToSpawn();
            await WaitForPlayersToBeReady();
            await WaitForHostToStart();

            method_5(botsSettings, SpawnSystem, runCallback);
        }

        class PlayerLoopSystemType
        {
            public PlayerLoopSystemType()
            {

            }
        }


        public async Task SpawnLoot(LocationSettingsClass.Location location)
        {
            Logger.LogDebug(nameof(SpawnLoot));

            using (TokenStarter.StartWithToken("SpawnLoot"))
            {
                Item[] source = location.Loot.Select((GLootItem x) => x.Item).ToArray();
                ResourceKey[] array = source.OfType<ContainerCollection>().GetAllItemsFromCollections().Concat(source.Where((Item x) => !(x is ContainerCollection))).SelectMany((Item x) => x.Template.AllResources)
                    .ToArray();
                if (array.Length != 0)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    //var parentPlayerLoopSystemType = ReflectionHelpers.EftTypes.FirstOrDefault(x => ReflectionHelpers.GetMethodForType(x, "FindParentPlayerLoopSystem") != null);

                    PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
                    var index = 0;
                    PlayerLoopSystemHelpers.FindParentPlayerLoopSystem(currentPlayerLoop, typeof(EarlyUpdate.UpdateTextureStreamingManager), out playerLoopSystem, out index);
                    PlayerLoopSystem[] array2 = new PlayerLoopSystem[playerLoopSystem.subSystemList.Length];
                    if (index != -1)
                    {
                        Array.Copy(playerLoopSystem.subSystemList, array2, playerLoopSystem.subSystemList.Length);
                        PlayerLoopSystem playerLoopSystem2 = default(PlayerLoopSystem);
                        playerLoopSystem2.updateDelegate = smethod_3;
                        playerLoopSystem2.type = typeof(PlayerLoopSystemType);
                        PlayerLoopSystem playerLoopSystem3 = playerLoopSystem2;
                        playerLoopSystem.subSystemList[index] = playerLoopSystem3;
                        PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                    }
                    await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, array, JobPriority.General, new BundleLoaderProgress(p =>
                    {
                        SetMatchmakerStatus("Loading loot... " + p.Stage, p.Progress);
                    }));
                    if (index != -1)
                    {
                        Array.Copy(array2, playerLoopSystem.subSystemList, playerLoopSystem.subSystemList.Length);
                        PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                    }
                }
                LootItems lootItems = Singleton<GameWorld>.Instance.method_4(location.Loot);
                Singleton<GameWorld>.Instance.method_5(lootItems, initial: true);
                await gparam_0.Player.ManageGameQuests();
            }
        }

        private async Task<EFT.Player> CreatePlayerSpawn()
        {
            Logger.LogDebug(nameof(CreatePlayerSpawn));

            int playerId = 1;
            EFT.Player.EUpdateMode armsUpdateMode = EFT.Player.EUpdateMode.Auto;
            var obj = await vmethod_2(playerId, Vector3.zero, Quaternion.identity, "Player", "", EPointOfView.FirstPerson, Profile_0, aiControl: false, base.UpdateQueue, armsUpdateMode, EFT.Player.EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.ClientPlayerMode, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
            // handled by CoopPlayer.Create
            , null
            // handled by CoopPlayer.Create
            , null
            // handled by CoopPlayer.Create
            , null
            );
            obj.Location = Location_0.Id;
            obj.OnEpInteraction += base.OnEpInteraction;
            return obj;
        }
    }
}
