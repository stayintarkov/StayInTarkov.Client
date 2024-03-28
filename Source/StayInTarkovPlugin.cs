using Aki.Core.Patches;
using Aki.Custom.Airdrops.Patches;
using Aki.Custom.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT.Communications;
using EFT.UI;
using Newtonsoft.Json.Linq;
using StayInTarkov.AkiSupport.Custom;
using StayInTarkov.AkiSupport.SITFixes;
using StayInTarkov.Configuration;
using StayInTarkov.Coop;
using StayInTarkov.Coop.AI;
using StayInTarkov.Coop.Player.Health;
using StayInTarkov.EssentialPatches;
using StayInTarkov.EssentialPatches.Web;
using StayInTarkov.FileChecker;
using StayInTarkov.Health;
using StayInTarkov.Networking;
using StayInTarkov.ThirdParty;
using StayInTarkov.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BepInEx.Configuration;
using UnityEngine;

namespace StayInTarkov
{
    /// <summary>
    /// Stay in Tarkov Plugin. 
    /// Written by: Paulov
    /// Used template by BepInEx
    /// </summary>
    [BepInPlugin("com.stayintarkov", "StayInTarkov", "1.10.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class StayInTarkovPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// Stores the Instance of this Plugin
        /// </summary>
        public static StayInTarkovPlugin Instance;

        /// <summary>
        /// A static location to obtain SIT settings from the BepInEx .cfg
        /// </summary>
        public static PluginConfigSettings Settings { get; private set; }

        /// <summary>
        /// If any mod dependencies fail, show an error. This is a flag to say it has occurred.
        /// </summary>
        private bool ShownDependancyError { get; set; }

        /// <summary>
        /// This is the Official EFT Version defined by BSG
        /// </summary>
        public static string EFTVersionMajor { get; internal set; }

        /// <summary>
        /// This is the EFT Version defined by BSG that is found in the Assembly
        /// </summary>
        public static string EFTAssemblyVersion { get; internal set; }

        /// <summary>
        /// This is the EFT Version defined by BSG that is found in the Executable file
        /// </summary>
        public static string EFTEXEFileVersion { get; internal set; }

        public static JObject LanguageDictionary { get; } = new JObject();

        public static bool LanguageDictionaryLoaded { get; private set; }


        internal static string IllegalMessage { get; }
            = LanguageDictionaryLoaded && LanguageDictionary.ContainsKey("ILLEGAL_MESSAGE")
                ? LanguageDictionary["ILLEGAL_MESSAGE"].ToString()
                : "Illegal game found. Please buy, install and launch the game once.";


        void Awake()
        {
            Instance = this;
            Settings = new PluginConfigSettings(Logger, Config);
            LogDependancyErrors();

            // Gather the Major/Minor numbers of EFT ASAP
            new VersionLabelPatch(Config).Enable();
            StartCoroutine(VersionChecks());
            SITIPAddressManager.GetExternalIPAddress();

            ReadInLanguageDictionary();

            EnableCorePatches();
            EnableBundlePatches();


            EnableSPPatches();

            EnableCoopPatches();

            EnableAirdropPatches();

            ThirdPartyModPatches.Run(Config, this);

            Logger.LogInfo($"Stay in Tarkov is loaded!");
        }


        private void EnableBundlePatches()
        {
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

        private void EnableAirdropPatches()
        {
            //new AirdropPatch().Enable();
            new AirdropFlarePatch().Enable();
        }

        private bool shownCheckError = false;
        private bool bsgThanksShown = false;

        void Update()
        {
            if (!LegalGameCheck.Checked)
                LegalGameCheck.LegalityCheck(Config);

            if (Singleton<PreloaderUI>.Instantiated
                && !shownCheckError
                && LegalGameCheck.LegalGameFound[0] != 0x1
                && LegalGameCheck.LegalGameFound[1] != 0x0
               )
            {
                shownCheckError = true;
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("", StayInTarkovPlugin.IllegalMessage,
                    ErrorScreen.EButtonType.QuitButton, 60, () => { Application.Quit(); },
                    () => { Application.Quit(); });
            }
            else
            {
                if (!bsgThanksShown)
                {
                    bsgThanksShown = true;
                    StayInTarkovHelperConstants.Logger.LogInfo("Official EFT Found. Thanks for supporting BSG.");
                }
            }
        }

        private void ReadInLanguageDictionary()
        {
            var currentCultureName = Thread.CurrentThread.CurrentCulture.Name;
            Logger.LogDebug($"Current Culture: {currentCultureName}");

            ConfigEntry<LanguageList.Language> userLanguage = Config.Bind("SIT.Localization", "Language",
                LanguageList.ByCultureName(currentCultureName),
                new ConfigDescription("Language",
                    null));

            Logger.LogDebug($"User Language: {userLanguage.Value}");

            var languageFiles = new List<string>();
            foreach (var mrs in typeof(StayInTarkovPlugin).Assembly.GetManifestResourceNames()
                         .Where(x => x.StartsWith("StayInTarkov.Resources.Language")))
            {
                languageFiles.Add(mrs);
                Logger.LogDebug($"Loaded Language File: {mrs}");
            }

            Logger.LogDebug(userLanguage.Value);

            Stream stream;
            if (Enum.IsDefined(typeof(LanguageList.Language), userLanguage.Value))
            {
                LanguageList.LanguageInfo languageInfo = LanguageList.ByLanguage(userLanguage.Value);
                stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(
                    languageFiles.First(x => x.EndsWith(languageInfo.FileName)));
            }
            else
            {
                stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(
                    languageFiles.First(x => x.EndsWith(LanguageList.Default.FileName)));
            }

            if (stream == null)
                return;

            StreamReader sr;
            string str;
            JObject resultLocaleDictionary;
            // Load Language Stream in
            using (sr = new StreamReader(stream))
            {
                str = sr.ReadToEnd();

                resultLocaleDictionary = JObject.Parse(str);

                foreach (var kvp in resultLocaleDictionary)
                {
                    LanguageDictionary.Add(kvp.Key, kvp.Value);
                }
            }

            // Load English Language Stream to Fill any missing expected statements in the Dictionary
            using (sr = new StreamReader(
                       typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(
                           languageFiles.First(x => x.EndsWith(LanguageList.Default.FileName)))))
            {
                foreach (var kvp in JObject.Parse(sr.ReadToEnd()))
                {
                    if (!LanguageDictionary.ContainsKey(kvp.Key))
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
                        var majorN2 = EFTVersionMajor.Split('.')[1]; // 14
                        var majorN3 = EFTVersionMajor.Split('.')[2]; // 1
                        var majorN4 = EFTVersionMajor.Split('.')[3]; // 3
                        var majorN5 = EFTVersionMajor.Split('.')[4]; // build number

                        if (majorN1 != "0" || majorN2 != "14" || majorN3 != "1" || majorN4 != "3")
                        {
                            Logger.LogError(
                                "Version Check: This version of SIT is not designed to work with this version of EFT.");
                        }
                        else
                        {
                            Logger.LogInfo("Version Check: OK.");
                        }
                    }

                    break;
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
                    new WebSocketPatch().Enable();
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
                //new RemoveScavModeButtonPatch().Enable();

                //// --------- READY Button ---------------------
                new RemoveReadyButtonPatch().Enable();

                //// --------- Airdrop -----------------------
                //new AirdropPatch().Enable();

                //// --------- Settings -----------------------
                SettingsLocationPatch.Enable();

                //// --------- Screens ----------------
                EnableSPPatches_Screens(Config);

                //// --------- Progression -----------------------
                EnableSPPatches_PlayerProgression();

                //// --------------------------------------
                // Bots
                EnableSPPatches_Bots(Config);

                new QTEPatch().Enable();
            }
            catch (Exception ex)
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
            new PHC_HandleFall_Patch().Enable();
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
            new FixBrokenSpawnOnSandboxPatch().Enable();
            new IsEnemyPatch().Enable();
        }

        private void EnableCoopPatches()
        {
            CoopPatches.Run(Config);
        }

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

            DisplayMessageNotifications.DisplayMessageNotification($"{errorMessage}",
                ENotificationDurationType.Infinite, ENotificationIconType.Alert, UnityEngine.Color.red);

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