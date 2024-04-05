using BepInEx;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Healing;
using StayInTarkov.AkiSupport.Singleplayer.Patches.MainMenu;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Progression;
using StayInTarkov.AkiSupport.Singleplayer.Patches.Quests;
using StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix;
using StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode;
using System;

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
                new ExperienceGainPatch().Enable();
                new MidRaidQuestChangePatch().Enable();
                new MidRaidAchievementChangePatch().Enable();
                new InsuranceScreenPatch().Enable();
                new GetNewBotTemplatesPatch().Enable();
                new DogtagPatch().Enable();
                new PostRaidHealingPricePatch().Enable();
                new EndByTimerPatch().Enable();
                new PostRaidHealScreenPatch().Enable();
                new LighthouseBridgePatch().Enable();
                new LighthouseTransmitterPatch().Enable();
                new LabsKeycardRemovalPatch().Enable();
                new AmmoUsedCounterPatch().Enable();
                new ArmorDamageCounterPatch().Enable();
                
                // Scav Patches
                new ScavExperienceGainPatch().Enable();
                new ScavPrefabLoadPatch().Enable();
                new ScavProfileLoadPatch().Enable();
                new ScavExfilPatch().Enable();
                new ScavLateStartPatch().Enable();
                new ExfilPointManagerPatch().Enable();
                new ScavProfileLoadCoopPatch().Enable();
                new ScavQuestPatch().Enable();
                new ScavSellAllRequestPatch().Enable();
                new ScavSellAllPriceStorePatch().Enable();
                new ScavEncyclopediaPatch().Enable();
                new ScavItemCheckmarkPatch().Enable();
                new IsHostileToEverybodyPatch().Enable();
                new ScavRepAdjustmentPatch().Enable();
                
                // Unused Patches
                //new OfflineSaveProfilePatch().Enable();
                //new OfflineSpawnPointPatch().Enable();
                //new MainMenuControllerPatch().Enable();
                //new PlayerPatch().Enable();
                //new SelectLocationScreenPatch().Enable();
                //new BotTemplateLimitPatch().Enable();
                //new RemoveUsedBotProfilePatch().Enable();
                //new TinnitusFixPatch().Enable();
                //new MaxBotPatch().Enable();
                //new SpawnPmcPatch().Enable();
                //new VoIPTogglerPatch().Enable();
                //new MidRaidQuestChangePatch().Enable();
                //new HealthControllerPatch().Enable();
                //new EmptyInfilFixPatch().Enable();
                //new SmokeGrenadeFuseSoundFixPatch().Enable();
                //new PlayerToggleSoundFixPatch().Enable();
                //new PluginErrorNotifierPatch().Enable();
                //new SpawnProcessNegativeValuePatch().Enable();
                //new InsuredItemManagerStartPatch().Enable();
                //new MapReadyButtonPatch().Enable();
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
