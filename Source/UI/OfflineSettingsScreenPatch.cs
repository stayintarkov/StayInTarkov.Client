using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.UI
{
    public class OfflineSettingsScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MatchmakerOfflineRaidScreen);
            var desiredMethod = ReflectionHelpers.GetAllMethodsForType(desiredType).Single(x => x.Name == "Show" && x.GetParameters().Length == 2);

            //Logger.LogInfo($"{GetType().Name} Type: {desiredType?.Name}");
            //Logger.LogInfo($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        public static bool Prefix(
            MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
            , DefaultUIButton ____nextButtonSpawner
           )
        {
            //Logger.LogInfo(JsonConvert.SerializeObject(raidSettings));



            raidSettings.RaidMode = ERaidMode.Local;
            RemoveBlockers(__instance
              , profileInfo
              , raidSettings
              , ____offlineModeToggle
              , ____changeSettingsButton
              , ____onlineBlocker
              , ____readyButton
              , ____nextButtonSpawner
              );
            //return false;
            return true;
        }

        [PatchPostfix]
        public static void PatchPostfix(
           MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
            , DefaultUIButton ____nextButtonSpawner
            )
        {
            var warningPanel = GameObject.Find("WarningPanelHorLayout");
            warningPanel?.SetActive(false);
            var settingslayoutcon = GameObject.Find("NonLayoutContainer");
            settingslayoutcon?.SetActive(false);
            var settingslist = GameObject.Find("RaidSettingsSummary");
            settingslist?.SetActive(false);
            RemoveBlockers(__instance
             , profileInfo
             , raidSettings
             , ____offlineModeToggle
             , ____changeSettingsButton
             , ____onlineBlocker
             , ____readyButton
             , ____nextButtonSpawner
             );

            ____changeSettingsButton?.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(null) { });

            //Logger.LogInfo("AutoSetOfflineMatch2.Postfix");

        }

        public static void RemoveBlockers(
            MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
            , DefaultUIButton ____nextButtonSpawner
            )
        {
            raidSettings.RaidMode = ERaidMode.Local;
            //raidSettings.BotSettings.IsEnabled = true;
            raidSettings.Side = ESideType.Pmc;
            raidSettings.BotSettings.BossType = EFT.Bots.EBossType.AsOnline;
            raidSettings.WavesSettings.IsBosses = true;
            //raidSettings.WavesSettings.BotAmount = EFT.Bots.EBotAmount.Low; // Low seemed too low.
            raidSettings.WavesSettings.BotAmount = EFT.Bots.EBotAmount.Medium;

            ____onlineBlocker.RemoveBlock();
            ____onlineBlocker.enabled = false;
            ____offlineModeToggle.isOn = true;
            ____offlineModeToggle.enabled = false;
            ____offlineModeToggle.interactable = false;
            ____changeSettingsButton.Interactable = false;
            ____changeSettingsButton.enabled = false;
            ____readyButton.Interactable = false;
            ____readyButton.enabled = false;

            //____nextButtonSpawner.OnClick.Invoke();
            //Logger.LogInfo("AutoSetOfflineMatch2.RemoveBlockers");
        }
    }

}