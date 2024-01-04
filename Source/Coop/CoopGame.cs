using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Game.Spawning;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.MovingPlatforms;
using EFT.UI;
using EFT.Weather;
using JsonType;
using Newtonsoft.Json;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.FreeCamera;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace StayInTarkov.Coop
{
    public sealed class FriendlyAIPMCSystem
    {
        /// <summary>
        /// This spawns bots but does nothing. DO NOT USE!
        /// </summary>
        [JsonProperty("shouldSpawnFriendlyAI")]
        public bool? ShouldSpawnFriendlyAI { get; set; } = false;

        public int? CurrentNumberOfFriendlies { get; set; } = 0;

        [JsonProperty("maxNumberOfFriendlies")]
        public int? MaxNumberOfFriendlies { get; set; } = 1;
    }

    /// <summary>
    /// A custom Game Type
    /// </summary>
    internal sealed class CoopGame : BaseLocalGame<GamePlayerOwner>, IBotGame, ISITGame
    {

        public new bool InRaid { get { return true; } }

        public FriendlyAIPMCSystem FriendlyAIPMCSystem { get; set; } = new FriendlyAIPMCSystem();

        public ISession BackEndSession { get { return StayInTarkovHelperConstants.BackEndSession; } }

        BotsController IBotGame.BotsController
        {
            get
            {
                if (BotsController == null)
                {
                    BotsController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(base.GetType(), typeof(BotsController)).GetValue(this);
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
                    BotsController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(base.GetType(), typeof(BotsController)).GetValue(this);
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

        private static ManualLogSource Logger;


        // Token: 0x0600844F RID: 33871 RVA: 0x0025D580 File Offset: 0x0025B780
        internal static CoopGame Create(
            InputTree inputTree
            , Profile profile
            , GameDateTime backendDateTime
            , Insurance insurance
            , MenuUI menuUI
            , CommonUI commonUI
            , PreloaderUI preloaderUI
            , GameUI gameUI
            , LocationSettings.Location location
            , TimeAndWeatherSettings timeAndWeather
            , WavesSettings wavesSettings
            , EDateTime dateTime
            , Callback<ExitStatus, TimeSpan, ClientMetrics> callback
            , float fixedDeltaTime
            , EUpdateQueue updateQueue
            , ISession backEndSession
            , TimeSpan sessionTime)
        {
            BotsController = null;

            Logger = BepInEx.Logging.Logger.CreateLogSource("Coop Game Mode");
            Logger.LogInfo("CoopGame.Create");

            if (wavesSettings.BotAmount == EBotAmount.NoBots && MatchmakerAcceptPatches.IsServer)
                wavesSettings.BotAmount = EBotAmount.Medium;

            CoopGame coopGame = BaseLocalGame<GamePlayerOwner>
                .smethod_0<CoopGame>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));

            // ---------------------------------------------------------------------------------
            // Non Waves Scenario setup
            coopGame.nonWavesSpawnScenario_0 = (NonWavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] { coopGame, location, coopGame.PBotsController });
            coopGame.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            // ---------------------------------------------------------------------------------
            // Waves Scenario setup
            coopGame.wavesSpawnScenario_0 = (WavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] {
                    coopGame.gameObject
                    , location.waves
                    , new Action<BotSpawnWave>((wave) => coopGame.PBotsController.ActivateBotsByWave(wave))
                    , location });

            // ---------------------------------------------------------------------------------
            // Setup Boss Wave Manager
            var bosswavemanagerValue = ReflectionHelpers.GetMethodForType(typeof(BossWaveManager), "smethod_0").Invoke
                (null, new object[] { location.BossLocationSpawn, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }) });
            ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(CoopGame), typeof(BossWaveManager)).SetValue(coopGame, bosswavemanagerValue);
            coopGame.BossWaveManager = bosswavemanagerValue as BossWaveManager;

            coopGame.func_1 = (EFT.Player player) => GamePlayerOwner.Create<GamePlayerOwner>(player, inputTree, insurance, backEndSession, commonUI, preloaderUI, gameUI, coopGame.GameDateTime, location);

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
            // TODO: Switch to GameClientTCP/GameClientUDP
            AkiBackendCommunication.Instance.WebSocketCreate(MatchmakerAcceptPatches.Profile);


            return coopGame;
        }

        //BossLocationSpawn[] bossSpawnAdjustments;

        public void CreateCoopGameComponent()
        {
            var coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent != null)
            {
                GameObject.Destroy(coopGameComponent);
            }

            if (CoopPatches.CoopGameComponentParent != null)
            {
                GameObject.Destroy(CoopPatches.CoopGameComponentParent);
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
            if (!string.IsNullOrEmpty(MatchmakerAcceptPatches.GetGroupId()))
            {
                Logger.LogDebug($"{nameof(CreateCoopGameComponent)}:{MatchmakerAcceptPatches.GetGroupId()}");
                coopGameComponent.ServerId = MatchmakerAcceptPatches.GetGroupId();
                coopGameComponent.Timestamp = MatchmakerAcceptPatches.GetTimestamp();
            }
            else
            {
                GameObject.Destroy(coopGameComponent);
                coopGameComponent = null;
                Logger.LogError("========== ERROR = COOP ========================");
                Logger.LogError("No Server Id found, Deleting Coop Game Component");
                Logger.LogError("================================================");
                throw new Exception("No Server Id found");
            }

            if (MatchmakerAcceptPatches.IsServer)
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
                AkiBackendCommunication.Instance.PostDownWebSocketImmediately("");

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
                AkiBackendCommunication.Instance.SendDataToPool(hostPingerPacket.ToJson());
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
                    AkiBackendCommunication.Instance.SendDataToPool(raidTimerDict.ToJson());
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
                    AkiBackendCommunication.Instance.SendDataToPool(packet);
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
                    AkiBackendCommunication.Instance.SendDataToPool(dict.ToJson());
                }
            }
        }

        public Dictionary<string, EFT.Player> Bots { get; } = new ();

        private async Task<LocalPlayer> CreatePhysicalBot(Profile profile, Vector3 position)
        {
            if (MatchmakerAcceptPatches.IsClient)
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

            LocalPlayer localPlayer;
            if (!base.Status.IsRunned())
            {
                localPlayer = null;
            }
            else if (this.Bots.ContainsKey(profile.Id))
            {
                localPlayer = null;
            }
            else
            {
                int num = 999 + Bots.Count;
                profile.SetSpawnedInSession(profile.Info.Side == EPlayerSide.Savage);

                localPlayer
                   = (await CoopPlayer.Create(
                       num
                       , position
                       , Quaternion.identity
                       , "Player"
                       , ""
                       , EPointOfView.ThirdPerson
                       , profile
                       , true
                       , base.UpdateQueue
                       , EFT.Player.EUpdateMode.Manual
                       , EFT.Player.EUpdateMode.Auto
                       , BackendConfigManager.Config.CharacterController.BotPlayerMode
                    , () => 1f
                    , () => 1f
                    , FilterCustomizationClass1.Default
                    )
                  );
                localPlayer.Location = base.Location_0.Id;
                if (this.Bots.ContainsKey(localPlayer.ProfileId))
                {
                    GameObject.Destroy(localPlayer);
                    return null;
                }
                else
                {
                    this.Bots.Add(localPlayer.ProfileId, localPlayer);
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
                coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)localPlayer);
                coopGameComponent.ProfileIdsAI.Add(profile.Id);


            }
            return localPlayer;
        }


        //public async Task<LocalPlayer> CreatePhysicalPlayer(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, QuestControllerClass questController)
        //{
        //    profile.SetSpawnedInSession(value: false);
        //    return await LocalPlayer.Create(playerId, position, rotation, "Player", "", EPointOfView.FirstPerson, profile, aiControl: false, base.UpdateQueue, armsUpdateMode, EFT.Player.EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.ClientPlayerMode, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity, new StatisticsManagerForPlayer1(), new FilterCustomizationClass(), questController, isYourPlayer: true);
        //}

        public string InfiltrationPoint;

        public override void vmethod_0()
        {
        }

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
            if (!MatchmakerAcceptPatches.IsClient)
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
            else if (MatchmakerAcceptPatches.IsClient)
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
        public override async Task<LocalPlayer> vmethod_2(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestController questController, AbstractAchievementsController achievementsController)
        {
            spawnPoints = SpawnPoints.CreateFromScene(DateTime.Now, base.Location_0.SpawnPointParams);
            int spawnSafeDistance = ((Location_0.SpawnSafeDistanceMeters > 0) ? Location_0.SpawnSafeDistanceMeters : 100);
            SpawnSystemSettings settings = new(Location_0.MinDistToFreePoint, Location_0.MaxDistToFreePoint, Location_0.MaxBotPerZone, spawnSafeDistance);
            SpawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, () => UnityEngine.Time.time, Singleton<GameWorld>.Instance, PBotsController, spawnPoints);
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
               , base.UpdateQueue
               , armsUpdateMode
               , EFT.Player.EUpdateMode.Auto
               , BackendConfigManager.Config.CharacterController.ClientPlayerMode
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
               , new FilterCustomizationClass()
               , questController
               , isYourPlayer: true);
            profile.SetSpawnedInSession(value: false);
            if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogDebug($"{nameof(vmethod_2)}:Unable to find {nameof(CoopGameComponent)}");
                await Task.Delay(5000);
            }
            coopGameComponent.Players.TryAdd(profile.Id, (CoopPlayer)myPlayer);
            coopGameComponent.ProfileIdsUser.Add(profile.Id);

            //SendOrReceiveSpawnPoint(myPlayer);

            // ---------------------------------------------
            // Here we can wait for other players, if desired
            await Task.Run(async () =>
            {
                if (coopGameComponent != null)
                {
                    while (coopGameComponent.PlayerUsers == null)
                    {
                        Logger.LogDebug($"{nameof(vmethod_2)}: {nameof(coopGameComponent.PlayerUsers)} is null");
                        await Task.Delay(1000);
                    }

                    var numbersOfPlayersToWaitFor = MatchmakerAcceptPatches.HostExpectedNumberOfPlayers - coopGameComponent.PlayerUsers.Count();
                    do
                    {
                        if (coopGameComponent.PlayerUsers == null)
                        {
                            Logger.LogDebug($"{nameof(vmethod_2)}: {nameof(coopGameComponent.PlayerUsers)} is null");
                            await Task.Delay(1000);
                            continue;
                        }

                        if (coopGameComponent.PlayerUsers.Count() == 0)
                        {
                        Logger.LogDebug($"{nameof(vmethod_2)}: {nameof(coopGameComponent.PlayerUsers)} is empty");
                            await Task.Delay(1000);
                            continue;
                        }

                        var progress = (coopGameComponent.PlayerUsers.Count() / MatchmakerAcceptPatches.HostExpectedNumberOfPlayers);
                        numbersOfPlayersToWaitFor = MatchmakerAcceptPatches.HostExpectedNumberOfPlayers - coopGameComponent.PlayerUsers.Count();
                        if (MatchmakerAcceptPatches.TimeHasComeScreenController != null)
                        {
                            MatchmakerAcceptPatches.TimeHasComeScreenController.ChangeStatus($"Waiting for {numbersOfPlayersToWaitFor} Player(s)", progress);
                        }

                        await Task.Delay(1000);

                    } while (numbersOfPlayersToWaitFor > 0);
                }
            });

            // ---------------------------------------------

            CoopPatches.EnableDisablePatches();

            // ---------------------------------------------
            // Create friendly bots
            if (FriendlyAIPMCSystem != null
                && FriendlyAIPMCSystem.ShouldSpawnFriendlyAI.HasValue
                && FriendlyAIPMCSystem.ShouldSpawnFriendlyAI.Value
                && FriendlyAIPMCSystem.MaxNumberOfFriendlies.HasValue
                && FriendlyAIPMCSystem.MaxNumberOfFriendlies.Value > 0)
            {
                for (var indexOfFriendly = 0; indexOfFriendly < FriendlyAIPMCSystem.MaxNumberOfFriendlies.Value; indexOfFriendly++)
                {
                    var profileClone = profile.Clone();
                    profileClone.AccountId = new System.Random().Next(1000000000, int.MaxValue).ToString();
                    profileClone.Id = "ai" + new MongoID(true);
                    profileClone.Skills.StartClientMode();

                    CoopPlayer friendlyBot = (CoopPlayer)(await CoopPlayer
                       .Create(
                       playerId + 90 + indexOfFriendly
                       , position
                       , rotation
                       , "Player"
                       , ""
                       , EPointOfView.ThirdPerson
                       , profileClone
                       , aiControl: true
                       , base.UpdateQueue
                       , armsUpdateMode
                       , EFT.Player.EUpdateMode.Auto
                       , BackendConfigManager.Config.CharacterController.BotPlayerMode
                       , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                       , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                       , new FilterCustomizationClass()
                       , null
                       , isYourPlayer: false));
                    friendlyBot.IsFriendlyBot = true;
                    //var companionComponent = friendlyBot.GetOrAddComponent<SITCompanionComponent>();
                    //companionComponent.CoopPlayer = friendlyBot;
                    if (!FriendlyPlayers.ContainsKey(profileClone.Id))
                        FriendlyPlayers.Add(profileClone.Id, friendlyBot);


                }
            }

            return myPlayer;
            //return base.vmethod_2(playerId, position, rotation, layerName, prefix, pointOfView, profile, aiControl, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, statisticsManager, questController);
        }

        public static void SendPlayerDataToServer(EFT.LocalPlayer player)
        {
            var profileJson = player.Profile.SITToJson();


            Dictionary<string, object> packet = new()
            {
                        {
                            "serverId",
                            MatchmakerAcceptPatches.GetGroupId()
                        },
                        {
                        "isAI",
                            //player.IsAI && player.AIData != null && player.AIData.IsAI && !player.IsYourPlayer
                            !player.IsYourPlayer
                        },
                        {
                            "profileId",
                            player.ProfileId
                        },
                        {
                            "groupId",
                            Matchmaker.MatchmakerAcceptPatches.GetGroupId()
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
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, packet);



            // ==================== TEST ==========================
            // TODO: Replace with Unit Tests
            var pJson = player.Profile.SITToJson();
            //Logger.LogDebug(pJson);
            var pProfile = pJson.SITParseJson<Profile>();
            Assert.AreEqual<Profile>(player.Profile, pProfile);


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

            var shouldSpawnBots = !MatchmakerAcceptPatches.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            if (!shouldSpawnBots)
            {
                controllerSettings.BotAmount = EBotAmount.NoBots;

                if (!PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem)
                    Logger.LogDebug("Bot Spawner System has been turned off - Wave System is Disabled");

                if (MatchmakerAcceptPatches.IsSinglePlayer)
                    Logger.LogDebug("Bot Spawner System has been turned off - You are running as Single Player");

                if (MatchmakerAcceptPatches.IsClient)
                    Logger.LogDebug("Bot Spawner System has been turned off - You are running as Client");
            }

            var nonwaves = (WaveInfo[])ReflectionHelpers.GetFieldFromTypeByFieldType(this.nonWavesSpawnScenario_0.GetType(), typeof(WaveInfo[])).GetValue(this.nonWavesSpawnScenario_0);

            LocalGameBotCreator profileCreator =
                new(BackEndSession
                , this.wavesSpawnScenario_0.SpawnWaves
                , Location_0.BossLocationSpawn
                , nonwaves
                , true);

            BotCreator botCreator = new(this, profileCreator, this.CreatePhysicalBot);
            BotZone[] botZones = LocationScene.GetAllObjects<BotZone>(false).ToArray<BotZone>();
            this.PBotsController.Init(this
                , botCreator
                , botZones
                , spawnSystem
                , this.wavesSpawnScenario_0.BotLocationModifier
                , controllerSettings.IsEnabled && controllerSettings.BotAmount != EBotAmount.NoBots
                , false // controllerSettings.IsScavWars
                , true
                , false
                , false
                , Singleton<GameWorld>.Instance
                , base.Location_0.OpenZones)
                ;

            Logger.LogInfo($"Location: {Location_0.Name}");

            MaxBotCount = Location_0.BotMax != 0 ? Location_0.BotMax : controllerSettings.BotAmount switch
            {
                EBotAmount.AsOnline => 10,
                EBotAmount.Low => 11,
                EBotAmount.Medium => 12,
                EBotAmount.High => 14,
                EBotAmount.Horde => 15,
                _ => 16,
            };
            switch(controllerSettings.BotAmount)
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

            this.PBotsController.SetSettings(numberOfBots, this.BackEndSession.BackEndConfig.BotPresets, this.BackEndSession.BackEndConfig.BotWeaponScatterings);
            this.PBotsController.AddActivePLayer(this.PlayerOwner.Player);

            //foreach (var friendlyB in FriendlyPlayers.Values)
            //{
            //    //BotOwner botOwner = BotOwner.Create(friendlyB, null, this.GameDateTime, this.botsController_0, true);
            //    //botOwner.GetComponentsInChildren<Collider>();
            //    //botOwner.GetPlayer.CharacterController.isEnabled = false;
            //    Logger.LogDebug("Attempting to Activate friendly bot");
            //    //botCreator.ActivateBot(friendlyB.Profile, botZones[0], false, (bot, zone) =>
            //    //{
            //    //    Logger.LogDebug("group action");
            //    //    return new BotsGroup(zone, this, bot, new List<BotOwner>(), new DeadBodiesController(new BotZoneGroupsDictionary()), this.Bots.Values.ToList(), forBoss: false);
            //    //}, (owner) =>
            //    //{

            //    //    Logger.LogDebug("Bot Owner created");

            //    //    owner.GetComponentsInChildren<Collider>();
            //    //    owner.GetPlayer.CharacterController.isEnabled = false;

            //    //}, cancellationToken: CancellationToken.None);
            //}

            yield return new WaitForSeconds(startDelay);
            if (shouldSpawnBots)
            {
                this.BossWaveManager.Run(EBotsSpawnMode.Anyway);

                //if (this.nonWavesSpawnScenario_0 != null)
                //    this.nonWavesSpawnScenario_0.Run();

                //Logger.LogDebug($"Running Wave Scenarios");

                if (this.wavesSpawnScenario_0.SpawnWaves != null && this.wavesSpawnScenario_0.SpawnWaves.Length != 0)
                {
                    Logger.LogDebug($"Running Wave Scenarios with Spawn Wave length : {this.wavesSpawnScenario_0.SpawnWaves.Length}");
                    this.wavesSpawnScenario_0.Run(EBotsSpawnMode.Anyway);
                }

            }
            else
            {
                if (this.wavesSpawnScenario_0 != null)
                    this.wavesSpawnScenario_0.Stop();
                if (this.nonWavesSpawnScenario_0 != null)
                    this.nonWavesSpawnScenario_0.Stop();
                if (this.BossWaveManager != null)
                    this.BossWaveManager.Stop();
            }



            yield return new WaitForEndOfFrame();
            Logger.LogInfo("vmethod_4.SessionRun");
            CreateExfiltrationPointAndInitDeathHandler();

            // No longer need this ping. Load complete and all other data should keep happening after this point.
            StopCoroutine(ClientLoadingPinger());
            //GCHelpers.ClearGarbage(emptyTheSet: true, unloadAssets: false);

            var magazines = Profile_0.Inventory.AllPlayerItems.OfType<MagazineClass>().ToList();
            for (int i = 0; i < magazines.Count(); i++)
                Profile_0.CheckMagazines(magazines[i].Id, 2);

            // Add FreeCamController to GameWorld GameObject
            Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<FreeCameraController>();
            Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<SITAirdropsManager>();

            // ------------------------------------------------------------------------
            // Setup Winter
            bool isWinter = BackEndSession.IsWinter;
            WinterEventController winterEventController = new WinterEventController();
            ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(GameWorld), typeof(WinterEventController)).SetValue(Singleton<GameWorld>.Instance, winterEventController);
            winterEventController.Run(isWinter).Wait();

            if (shouldSpawnBots)
            {
                if (this.nonWavesSpawnScenario_0 != null)
                    this.nonWavesSpawnScenario_0.Run();

                Logger.LogDebug($"Running Wave Scenarios");
            }
            yield break;
        }

        /// <summary>
        /// Died event handler
        /// </summary>
        public void CreateExfiltrationPointAndInitDeathHandler()
        {
            Logger.LogInfo("CreateExfiltrationPointAndInitDeathHandler");

            base.GameTimer.Start();
            //base.vmethod_5();
            gparam_0.vmethod_0();
            //gparam_0.Player.ActiveHealthController.DiedEvent += HealthController_DiedEvent;
            gparam_0.Player.HealthController.DiedEvent += HealthController_DiedEvent;

            InfiltrationPoint = spawnPoint.Infiltration;
            Profile_0.Info.EntryPoint = InfiltrationPoint;

            //Logger.LogDebug(InfiltrationPoint);

            ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(Location_0.exits, justLoadSettings: false, "");
            ExfiltrationPoint[] exfilPoints = ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0);
            base.GameUi.TimerPanel.SetTime(DateTime.UtcNow, Profile_0.Info.Side, base.GameTimer.SessionSeconds(), exfilPoints);
            foreach (ExfiltrationPoint exfiltrationPoint in exfilPoints)
            {
                exfiltrationPoint.OnStartExtraction += ExfiltrationPoint_OnStartExtraction;
                exfiltrationPoint.OnCancelExtraction += ExfiltrationPoint_OnCancelExtraction;
                exfiltrationPoint.OnStatusChanged += ExfiltrationPoint_OnStatusChanged;
                UpdateExfiltrationUi(exfiltrationPoint, contains: false, initial: true);
            }

            base.dateTime_0 = DateTime.UtcNow;
            base.Status = GameStatus.Started;
            ConsoleScreen.ApplyStartCommands();
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
            UpdateExfiltrationUi(point, point.Entered.Any((EFT.Player x) => x.ProfileId == Profile_0.Id));
            Logger.LogDebug("ExfiltrationPoint_OnStatusChanged");
            Logger.LogDebug(prevStatus);

            EExfiltrationStatus curStatus = point.Status;

            // Fixes player cannot extract with The Lab elevator and Armored Train
            if (prevStatus == EExfiltrationStatus.AwaitsManualActivation && curStatus == EExfiltrationStatus.Countdown)
                point.ExternalSetStatus(EExfiltrationStatus.RegularMode);
        }

        public ExitStatus MyExitStatus { get; set; } = ExitStatus.Survived;
        public string MyExitLocation { get; set; } = null;
        public ISpawnSystem SpawnSystem { get; set; }
        public int MaxBotCount { get; private set; }

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

            // Notify that I have left the Server
            AkiBackendCommunication.Instance.PostDownWebSocketImmediately(new Dictionary<string, object>() {
                { "m", "PlayerLeft" },
                { "profileId", Singleton<GameWorld>.Instance.MainPlayer.ProfileId },
                { "serverId", CoopGameComponent.GetServerId() }

            });

            // If I am the Host/Server, then ensure all the bots have left too
            if (MatchmakerAcceptPatches.IsServer)
            {
                foreach (var p in CoopGameComponent.GetCoopGameComponent().Players)
                {
                    AkiBackendCommunication.Instance.PostDownWebSocketImmediately(new Dictionary<string, object>() {

                            { "m", "PlayerLeft" },
                            { "profileId", p.Value.ProfileId },
                            { "serverId", CoopGameComponent.GetServerId() }

                        });
                }
            }


            if (this.BossWaveManager != null)
                this.BossWaveManager.Stop();

            if (this.nonWavesSpawnScenario_0 != null)
                this.nonWavesSpawnScenario_0.Stop();

            if (this.wavesSpawnScenario_0 != null)
                this.wavesSpawnScenario_0.Stop();


            CoopPatches.EnableDisablePatches();
            base.Stop(profileId, exitStatus, exitName, delay);
            CoopPatches.LeftGameDestroyEverything();
        }

        public override void CleanUp()
        {
            base.CleanUp();
            BaseLocalGame<GamePlayerOwner>.smethod_4(this.Bots);
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

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

        private Func<EFT.Player, GamePlayerOwner> func_1;


        public new void method_6(string backendUrl, string locationId, int variantId)
        {
            Logger.LogInfo("CoopGame:method_6");
            return;
        }
    }
}
