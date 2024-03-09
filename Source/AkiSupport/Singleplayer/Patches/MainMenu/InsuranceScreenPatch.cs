using EFT;
using HarmonyLib;
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
            return AccessTools.Method(typeof(MainMenuController), "method_72");
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
