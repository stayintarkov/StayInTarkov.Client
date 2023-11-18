using EFT;
using SIT.Tarkov.Core;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.MainMenu
{
    /// <summary>
    /// Credit: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/MainMenu/InsuranceScreenPatch.cs
    /// </summary>
    class InsuranceScreenPatch : ModulePatch
    {
        static InsuranceScreenPatch()
        {
            _ = nameof(MainMenuController.InventoryController);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MainMenuController);
            var desiredMethod = desiredType.GetMethod("method_69", BindingFlags.NonPublic | BindingFlags.Instance);

            Logger.LogDebug($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static void PrefixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Online;
        }

        [PatchPostfix]
        private static void PostfixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Local;

            var insuranceReadyButton = GameObject.Find("ReadyButton");
            if (insuranceReadyButton != null)
            {
                insuranceReadyButton.active = false;
            }
        }
    }
}
