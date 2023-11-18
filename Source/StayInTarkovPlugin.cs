using Aki.Core.Patches;
using Aki.Custom.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Communications;
using EFT.UI;
using Newtonsoft.Json;
using SIT.Core.AI.PMCLogic.Roaming;
using SIT.Core.AI.PMCLogic.RushSpawn;
using SIT.Core.AkiSupport.Airdrops;
using SIT.Core.AkiSupport.Custom;
using SIT.Core.AkiSupport.SITFixes;
using SIT.Core.Configuration;
using SIT.Core.Coop;
using SIT.Core.Coop.AI;
using SIT.Core.Core;
using SIT.Core.Core.FileChecker;
using SIT.Core.Other;
using SIT.Tarkov.Core;
using StayInTarkov.AkiSupport.Custom;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Healing;
using StayInTarkov.EssentialPatches;
using StayInTarkov.EssentialPatches.Web;
using StayInTarkov.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using StayInTarkov.Health;

namespace StayInTarkov
{

    /// <summary>
    /// Stay in Tarkov Plugin. 
    /// Written by: Paulov
    /// Used template by BepInEx
    /// </summary>
    [BepInPlugin("com.sit.core", "SIT.Core", "1.9.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class StayInTarkovPlugin : BaseUnityPlugin
    {
        public static StayInTarkovPlugin Instance;
        public static PluginConfigSettings Settings { get; private set; }

        private bool ShownDependancyError { get; set; }
        public static string EFTVersionMajor { get; internal set; }
        public static string EFTAssemblyVersion { get; internal set; }
        public static string EFTEXEFileVersion { get; internal set; }

        public static Dictionary<string, string> LanguageDictionary { get; } = new Dictionary<string, string>();    

        public static bool LanguageDictionaryLoaded { get; private set; }

        private void Awake()
        {
            Instance = this;
            Settings = new PluginConfigSettings(Logger, Config);
            LogDependancyErrors();

            // Gather the Major/Minor numbers of EFT ASAP
            new VersionLabelPatch(Config).Enable();
            StartCoroutine(VersionChecks());

            ReadInLanguageDictionary();

            EnableCorePatches();
            EnableSPPatches();
            EnableCoopPatches();
            ThirdPartyModPatches.Run(Config, this);

            Logger.LogInfo($"Stay in Tarkov is loaded!");
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private bool shownCheckError = false;

        void Update()
        {
            if (!LegalGameCheck.Checked) 
                LegalGameCheck.LegalityCheck(Config);

            if (Singleton<PreloaderUI>.Instantiated && !shownCheckError && !LegalGameCheck.LegalGameFound)
            {
                shownCheckError = true;
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("", LegalGameCheck.IllegalMessage, ErrorScreen.EButtonType.QuitButton, 60, () => { Application.Quit(); }, () => { Application.Quit(); });
            }
        }

        private void ReadInLanguageDictionary()
        {

            Logger.LogDebug(Thread.CurrentThread.CurrentCulture);

            var languageFiles = new List<string>();
            foreach (var mrs in typeof(StayInTarkovPlugin).Assembly.GetManifestResourceNames().Where(x => x.StartsWith("StayInTarkov.Resources.Language")))
            {
                languageFiles.Add(mrs);
                Logger.LogDebug(mrs);
            }

            Logger.LogDebug(Thread.CurrentThread.CurrentCulture.Name);
            var firstPartOfLang = Thread.CurrentThread.CurrentCulture.Name.ToLower().Substring(0, 2);
            Logger.LogDebug(firstPartOfLang);
            Stream stream = null;
            StreamReader sr = null;
            string str = null;
            Dictionary<string, string> resultLocaleDictionary = null;
            switch (firstPartOfLang)
            {
                case "zh":
                    switch (Thread.CurrentThread.CurrentCulture.Name.ToLower())
                    {
                        case "zh_TW":
                            stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("TraditionalChinese.json")));
                            break;
                        case "zh_CN":
                        default:
                            stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("SimplifiedChinese.json")));
                            break;
                    }
                    break;
                case "ja":
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("Japanese.json")));
                    break;
                case "de":
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("German.json")));
                    break;
                case "en":
                default:
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("English.json")));
                    break;

            }

            if (stream == null)
                return;

            // Load Language Stream in
            using (sr = new StreamReader(stream))
            {
                str = sr.ReadToEnd();

                resultLocaleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);

                if (resultLocaleDictionary == null)
                    return;

                foreach (var kvp in resultLocaleDictionary)
                {
                    LanguageDictionary.Add(kvp.Key, kvp.Value);
                }

               
            }

            // Load English Language Stream to Fill any missing expected statements in the Dictionary
            using (sr = new StreamReader(typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("English.json")))))
            {
                foreach (var kvp in JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd()))
                {
                    if(!LanguageDictionary.ContainsKey(kvp.Key))
                        LanguageDictionary.Add(kvp.Key, kvp.Value);
                }
            }

            Logger.LogDebug("Loaded in the following Language Dictionary");
            Logger.LogDebug(LanguageDictionary.ToJson());

            LanguageDictionaryLoaded = true;
        }

        private IEnumerator VersionChecks()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                if (!string.IsNullOrEmpty(EFTVersionMajor))
                {
                    Logger.LogInfo("Version Check: Detected:" + EFTVersionMajor);
                    if (EFTVersionMajor.Split('.').Length > 4)
                    {
                        var majorN1 = EFTVersionMajor.Split('.')[0]; // 0
                        var majorN2 = EFTVersionMajor.Split('.')[1]; // 13
                        var majorN3 = EFTVersionMajor.Split('.')[2]; // 1
                        var majorN4 = EFTVersionMajor.Split('.')[3]; // 1
                        var majorN5 = EFTVersionMajor.Split('.')[4]; // build number

                        // 0.13.5.2.26282
                        // 0.13.9.0.26921
                        if (majorN1 != "0" || majorN2 != "13" || majorN3 != "9" || majorN4 != "1")
                        {
                            Logger.LogError("Version Check: This version of SIT is not designed to work with this version of EFT.");
                        }
                        else
                        {
                            Logger.LogInfo("Version Check: OK.");
                        }
                    }

                    yield break;
                }
            }
        }

        private void EnableCorePatches()
        {
            Logger.LogInfo($"{nameof(EnableCorePatches)}");
            try
            {
                // File Checker
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new RunFilesCheckingPatch().Enable();
                // BattlEye
                new AntiCheatPatch().Enable();
                new BattlEyePatchFirstPassRun().Enable();
                new BattlEyePatchFirstPassUpdate().Enable();
                // Web Requests
                new SslCertificatePatch().Enable();
                new Aki.Core.Patches.UnityWebRequestPatch().Enable();
                new SendCommandsPatch().Enable();

                //https to http | wss to ws
                var url = DetectBackendUrlAndToken.GetBackendConnection().BackendUrl;
                if (!url.Contains("https"))
                {
                    new TransportPrefixPatch().Enable();
                    new Aki.Core.Patches.WebSocketPatch().Enable();
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            Logger.LogDebug($"{nameof(EnableCorePatches)} Complete");
        }

        private void EnableSPPatches()
        {
            try
            {
                var enabled = Config.Bind<bool>("SIT.SP", "Enable", true);
                if (!enabled.Value) // if it is disabled. stop all SIT SP Patches.
                {
                    Logger.LogInfo("SIT SP Patches has been disabled! Ignoring Patches.");
                    return;
                }

                //// --------- Player Init & Health -------------------
                EnableSPPatches_PlayerHealth(Config);

                //// --------- SCAV MODE ---------------------
                new RemoveScavModeButtonPatch().Enable();

                //// --------- Airdrop -----------------------
                //new AirdropPatch().Enable();

                //// --------- Screens ----------------
                EnableSPPatches_Screens(Config);

                //// --------- Progression -----------------------
                EnableSPPatches_PlayerProgression();

                //// --------------------------------------
                // Bots
                EnableSPPatches_Bots(Config);

                new QTEPatch().Enable();

                try
                {
                    BundleManager.GetBundles();
                    new EasyAssetsPatch().Enable();
                    new EasyBundlePatch().Enable();
                }
                catch (Exception ex)
                {
                    Logger.LogError("// --- ERROR -----------------------------------------------");
                    Logger.LogError("Bundle System Failed!!");
                    Logger.LogError(ex.ToString());
                    Logger.LogError("// --- ERROR -----------------------------------------------");
                }

            }
            catch(Exception ex)
            {
                Logger.LogError($"{nameof(EnableSPPatches)} failed.");
                Logger.LogError(ex);
            }
        }

        private static void EnableSPPatches_Screens(BepInEx.Configuration.ConfigFile config)
        {
            //new OfflineRaidMenuPatch().Enable();
            new OfflineSettingsScreenPatch().Enable();

        }

        private static void EnableSPPatches_PlayerProgression()
        {
            new OfflineDisplayProgressPatch().Enable();
            new OfflineSaveProfile().Enable();
        }

        private void EnableSPPatches_PlayerHealth(BepInEx.Configuration.ConfigFile config)
        {
            var enabled = config.Bind<bool>("SIT.SP", "EnableHealthPatches", true);
            if (!enabled.Value)
                return;

            new AssignHealthControllerToPlayerInRaid().Enable();
            new ChangeHealthPatch().Enable();
            new ChangeHydrationPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new OnDeadPatch(Config).Enable();
            new MainMenuControllerForHealthListenerPatch().Enable();
        }

        private static void EnableSPPatches_Bots(BepInEx.Configuration.ConfigFile config)
        {
            new CoreDifficultyPatch().Enable();
            new BotDifficultyPatch().Enable();
            new BotSettingsRepoClassIsFollowerFixPatch().Enable();
            new BotSelfEnemyPatch().Enable();
            new PmcFirstAidPatch().Enable();
            new SpawnProcessNegativeValuePatch().Enable();
            new LocationLootCacheBustingPatch().Enable();

            var enabled = config.Bind<bool>("SIT.SP", "EnableBotPatches", true);
            if (!enabled.Value)
                return;

            BrainManager.AddCustomLayer(typeof(RoamingLayer), new List<string>() { "Assault", "PMC", "sptUsec" }, 2);
            //BrainManager.AddCustomLayer(typeof(PMCRushSpawnLayer), new List<string>() { "Assault", "PMC" }, 9999);


        }

        private void EnableCoopPatches()
        {
            CoopPatches.Run(Config);
        }

        public static GameWorld gameWorld { get; private set; }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //GetPoolManager();
            //GetBackendConfigurationInstance();

            if (Singleton<GameWorld>.Instantiated)
                gameWorld = Singleton<GameWorld>.Instance;
        }

        //private void GetBackendConfigurationInstance()
        //{
        //    //if (
        //    //    PatchConstants.BackendStaticConfigurationType != null &&
        //    //    PatchConstants.BackendStaticConfigurationConfigInstance == null)
        //    //{
        //    //    PatchConstants.BackendStaticConfigurationConfigInstance = ReflectionHelpers.GetPropertyFromType(PatchConstants.BackendStaticConfigurationType, "Config").GetValue(null);
        //    //    //Logger.LogInfo($"BackendStaticConfigurationConfigInstance Type:{ PatchConstants.BackendStaticConfigurationConfigInstance.GetType().Name }");
        //    //}

        //    if (PatchConstants.BackendStaticConfigurationConfigInstance != null
        //        && PatchConstants.CharacterControllerSettings.CharacterControllerInstance == null
        //        )
        //    {
        //        PatchConstants.CharacterControllerSettings.CharacterControllerInstance
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(PatchConstants.BackendStaticConfigurationConfigInstance, "CharacterController", false);
        //        //Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
        //    }

        //    if (PatchConstants.CharacterControllerSettings.CharacterControllerInstance != null
        //        && PatchConstants.CharacterControllerSettings.ClientPlayerMode == null
        //        )
        //    {
        //        PatchConstants.CharacterControllerSettings.ClientPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ClientPlayerMode", false);

        //        PatchConstants.CharacterControllerSettings.ObservedPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ObservedPlayerMode", false);

        //        PatchConstants.CharacterControllerSettings.BotPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "BotPlayerMode", false);
        //    }

        //}


        private void LogDependancyErrors()
        {
            // Skip if we've already shown the message, or there are no errors
            if (ShownDependancyError || Chainloader.DependencyErrors.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Errors occurred during plugin loading");
            stringBuilder.AppendLine("-------------------------------------");
            stringBuilder.AppendLine();
            foreach (string error in Chainloader.DependencyErrors)
            {
                stringBuilder.AppendLine(error);
                stringBuilder.AppendLine();
            }
            string errorMessage = stringBuilder.ToString();

            DisplayMessageNotifications.DisplayMessageNotification($"{errorMessage}", ENotificationDurationType.Infinite, ENotificationIconType.Alert, UnityEngine.Color.red);

            // Show an error in the BepInEx console/log file
            Logger.LogError(errorMessage);

            // Show an error in the in-game console, we have to write this in reverse order because of the nature of the console output
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Trim().Length > 0)
                {
                    ConsoleScreen.LogError(line);
                }
            }

            ShownDependancyError = true;


        }

       

    }
}
