using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Game.Spawning;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.UI;
using EFT.Weather;
using JsonType;
using Newtonsoft.Json;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Core.AI.PMCLogic.Friendly.Companion;
using SIT.Core.Configuration;
using SIT.Core.Coop.Components;
using SIT.Core.Coop.FreeCamera;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace StayInTarkov.Coop
{
    public abstract class ASITGame : BaseLocalGame<GamePlayerOwner>, IBotGame
    {
        public new bool InRaid { get { return true; } }

        public ISession BackEndSession { get { return StayInTarkovHelperConstants.BackEndSession; } }

        BotControllerClass IBotGame.BotsController
        {
            get
            {
                if (botControllerClass == null)
                {
                    botControllerClass = (BotControllerClass)ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(BotControllerClass)).GetValue(this);
                }
                return botControllerClass;
            }
        }

        private static BotControllerClass botControllerClass;

        public BotControllerClass PBotsController
        {
            get
            {
                if (botControllerClass == null)
                {
                    botControllerClass = (BotControllerClass)ReflectionHelpers.GetFieldFromTypeByFieldType(GetType(), typeof(BotControllerClass)).GetValue(this);
                }
                return botControllerClass;
            }
        }

        public IWeatherCurve WeatherCurve
        {
            get
            {
                if (WeatherController.Instance != null)
                    return new WeatherCurve(new WeatherClass[1] { new WeatherClass() });

                return null;
            }
        }

        ManualLogSource Logger { get; set; }

        // Token: 0x0600844F RID: 33871 RVA: 0x0025D580 File Offset: 0x0025B780
        internal static T Create<T>(
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
            , TimeSpan sessionTime) where T : ASITGame
        {

            var r = 
               smethod_0<T>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
               , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));

            r.Logger = BepInEx.Logging.Logger.CreateLogSource("Coop Game Mode");
            r.Logger.LogInfo("CoopGame.Create");

            // Non Waves Scenario setup
            r.nonWavesSpawnScenario_0 = (NonWavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] { r, location, r.PBotsController });
            r.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            // Waves Scenario setup
            r.wavesSpawnScenario_0 = (WavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] {
                    r.gameObject
                    , location.waves
                    , new Action<Wave>((wave) => r.PBotsController.ActivateBotsByWave(wave))
                    , location });

            var bosswavemanagerValue = ReflectionHelpers.GetMethodForType(typeof(BossWaveManager), "smethod_0").Invoke
                (null, new object[] { location.BossLocationSpawn, new Action<BossLocationSpawn>((bossWave) => { r.PBotsController.ActivateBotsByWave(bossWave); }) });
            ReflectionHelpers.GetFieldFromTypeByFieldType(r.GetType(), typeof(BossWaveManager)).SetValue(r, bosswavemanagerValue);
            r.BossWaveManager = bosswavemanagerValue as BossWaveManager;

            r.func_1 = (player) => GamePlayerOwner.Create<GamePlayerOwner>(player, inputTree, insurance, backEndSession, commonUI, preloaderUI, gameUI, r.GameDateTime, location);

            return r;
        }

        public Dictionary<string, Player> Bots { get; set; } = new Dictionary<string, Player>();

        /// <summary>
        /// Matchmaker countdown
        /// </summary>
        /// <param name="timeBeforeDeploy"></param>
        public override void vmethod_1(float timeBeforeDeploy)
        {
            base.vmethod_1(timeBeforeDeploy);
        }

        /// <summary>
        /// Reconnection handling.
        /// </summary>
        public override void vmethod_3()
        {
            base.vmethod_3();
        }

        private BossWaveManager BossWaveManager;

        private WavesSpawnScenario wavesSpawnScenario_0;

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

        private Func<Player, GamePlayerOwner> func_1;


        public new void method_6(string backendUrl, string locationId, int variantId)
        {
            Logger.LogInfo("CoopGame:method_6");
            return;
        }
    }
}
