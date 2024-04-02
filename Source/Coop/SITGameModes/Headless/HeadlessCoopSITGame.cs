/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */

using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.CameraControl;
using EFT.InputSystem;
using EFT.UI;
using JsonType;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.SITGameModes.RemoteHosted
{
    public sealed class HeadlessCoopSITGame : CoopSITGame
    {
        public override string DisplayName => StayInTarkovPlugin.LanguageDictionary["MP_GAME"].ToString();

        internal static HeadlessCoopSITGame CreateHeadlessCoopSITGame(
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

            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(HeadlessCoopSITGame));
            Logger.LogInfo($"{nameof(HeadlessCoopSITGame)}.{nameof(CreateHeadlessCoopSITGame)}");

            //location.OfflineNewSpawn = false;
            //location.OfflineOldSpawn = true;
            //location.OldSpawn = true;

            HeadlessCoopSITGame sitgame =
                smethod_0<HeadlessCoopSITGame>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));


#if DEBUG
            Logger.LogDebug($"DEBUG:{nameof(backendDateTime)}:{backendDateTime.ToJson()}");
#endif
            sitgame.GameWorldTime = backendDateTime.Boolean_0 ? backendDateTime.DateTime_1 : backendDateTime.DateTime_0;
            Logger.LogDebug($"DEBUG:{nameof(sitgame.GameWorldTime)}:{sitgame.GameWorldTime}");


            // ---------------------------------------------------------------------------------
            // Non Waves Scenario setup
            Logger.LogDebug($"DEBUG:location");
            Logger.LogDebug($"{location.ToJson()}");

            WildSpawnWave[] waves = FixScavWaveSettings(wavesSettings, location.waves);
            sitgame.nonWavesSpawnScenario_0 = NonWavesSpawnScenario.smethod_0(sitgame, location, sitgame.PBotsController);
            sitgame.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            // ---------------------------------------------------------------------------------
            // Waves Scenario setup
            sitgame.wavesSpawnScenario_0 = WavesSpawnScenario.smethod_0(
                    sitgame.gameObject
                    , waves
                    , new Action<BotSpawnWave>((wave) => sitgame.PBotsController.ActivateBotsByWave(wave))
                    , location);

            // ---------------------------------------------------------------------------------
            // Setup Boss Wave Manager
            sitgame.BossWaves = sitgame.FixBossWaveSettings(wavesSettings, location);
            var bosswavemanagerValue = BossWaveManager.smethod_0(sitgame.BossWaves, new Action<BossLocationSpawn>((bossWave) => { sitgame.PBotsController.ActivateBotsByWave(bossWave); }));
            sitgame.BossWaveManager = bosswavemanagerValue;

            //coopGame.func_1 = (player) => GamePlayerOwner.Create<GamePlayerOwner>(player, inputTree, insurance, backEndSession, commonUI, preloaderUI, gameUI, coopGame.GameDateTime, location);

            // ---------------------------------------------------------------------------------
            // Setup ISITGame Singleton
            Singleton<ISITGame>.Create(sitgame);

            // ---------------------------------------------------------------------------------
            // Create Coop Game Component
            Logger.LogDebug($"{nameof(Create)}:Running {nameof(sitgame.CreateCoopGameComponent)}");
            sitgame.CreateCoopGameComponent();
            SITGameComponent.GetCoopGameComponent().LocalGameInstance = sitgame;



            // ---------------------------------------------------------------------------------
            // Create GameClient(s)
            switch (SITMatchmaking.SITProtocol)
            {
                case ESITProtocol.RelayTcp:
                    sitgame.GameClient = sitgame.GetOrAddComponent<GameClientTCPRelay>();

                    break;
                case ESITProtocol.PeerToPeerUdp:
                    sitgame.GameServer = sitgame.GetOrAddComponent<GameServerUDP>();
                    sitgame.GameClient = sitgame.GetOrAddComponent<GameClientUDP>();
                    break;
                default:
                    throw new Exception("Unknown SIT Protocol used!");

            }

            return sitgame;
        }

        public override Task<LocalPlayer> vmethod_2(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController)
        {
            return null;
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
            BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
            if (instance != null && instance.HalloweenSettings.EventActive && !instance.HalloweenSettings.LocationsToIgnore.Contains(location._Id))
            {
                GameObject gameObject = (GameObject)Resources.Load("Prefabs/HALLOWEEN_CONTROLLER");
                if (gameObject != null)
                {
                    GClass5.InstantiatePrefab(base.transform, gameObject);
                }
                else
                {
                    UnityEngine.Debug.LogError("Can't find event prefab in resources. Path : Prefabs/HALLOWEEN_CONTROLLER");
                }
            }
            BackendConfigManagerConfig config = BackendConfigManager.Config;
            if (config.FixedFrameRate > 0f)
            {
                base.FixedDeltaTime = 1f / config.FixedFrameRate;
            }
            await SpawnLoot(location);
            await WaitForPlayersToSpawn();
            await WaitForPlayersToBeReady();
            await WaitForHostToStart();
            method_5(botsSettings, SpawnSystem, runCallback);
        }
    }
}
