using System;
using BepInEx;
using StayInTarkov.AkiSupport.Singleplayer.Patches;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Healing;
using StayInTarkov.AkiSupport.Singleplayer.Patches.MainMenu;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Progression;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Quests;
using StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix;

namespace StayInTarkov.AkiSupport.Singleplayer
{
    /// <summary>
    /// Credit SPT-Aki team
    /// Paulov. I have removed a lot of unused patches
    /// </summary>
    [BepInPlugin("com.spt-aki.singleplayer", "AKI.Singleplayer", "1.0.0.0")]
    class AkiSingleplayerPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.SinglePlayer");

            try
            {
                //new OfflineSaveProfilePatch().Enable();
                //new OfflineSpawnPointPatch().Enable();
                new ExperienceGainPatch().Enable();
                //new ScavExperienceGainPatch().Enable();
                //new MainMenuControllerPatch().Enable();
                //new PlayerPatch().Enable();
                //new SelectLocationScreenPatch().Enable();
                new InsuranceScreenPatch().Enable();
                //new BotTemplateLimitPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                //new RemoveUsedBotProfilePatch().Enable();
                new DogtagPatch().Enable();
                //new LoadOfflineRaidScreenPatch().Enable();
                //new ScavPrefabLoadPatch().Enable();
                //new ScavProfileLoadPatch().Enable();
                //new ScavExfilPatch().Enable();
                //new ExfilPointManagerPatch().Enable();
                //new TinnitusFixPatch().Enable();
                //new MaxBotPatch().Enable();
                //new SpawnPmcPatch().Enable();
                new PostRaidHealingPricePatch().Enable();
                //new EndByTimerPatch().Enable();
                new PostRaidHealScreenPatch().Enable();
                //new VoIPTogglerPatch().Enable();
                //new MidRaidQuestChangePatch().Enable();
                //new HealthControllerPatch().Enable();
                new LighthouseBridgePatch().Enable();
                new LighthouseTransmitterPatch().Enable();
                //new EmptyInfilFixPatch().Enable();
                //new SmokeGrenadeFuseSoundFixPatch().Enable();
                //new PlayerToggleSoundFixPatch().Enable();
                //new PluginErrorNotifierPatch().Enable();
                //new SpawnProcessNegativeValuePatch().Enable();
                //new InsuredItemManagerStartPatch().Enable();
                //new MapReadyButtonPatch().Enable();
                new LabsKeycardRemovalPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.SinglePlayer");
        }
    }
}
