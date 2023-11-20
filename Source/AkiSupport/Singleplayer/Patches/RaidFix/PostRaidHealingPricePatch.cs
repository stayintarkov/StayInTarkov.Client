using EFT;
using HarmonyLib;
using System;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix
{

    /// <summary>
    /// Credit: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/RaidFix/PostRaidHealingPricePatch.cs
    /// </summary>
    public class PostRaidHealingPricePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(Profile.TraderInfo);
            var desiredMethod = desiredType.GetMethod("UpdateLevel", BindingFlags.NonPublic | BindingFlags.Instance);

            Logger.LogDebug($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        protected static void PatchPrefix(Profile.TraderInfo __instance)
        {
            if (__instance.Settings == null)
                return;

            if (__instance.Id == "bearTrader" || __instance.Id == "coopTrader" || __instance.Id == "usecTrader")
                return;

            var loyaltyLevel = __instance.Settings.GetLoyaltyLevel(__instance);
            var loyaltyLevelSettings = __instance.Settings.GetLoyaltyLevelSettings(loyaltyLevel);

            if (loyaltyLevelSettings == null)
                throw new IndexOutOfRangeException($"Loyalty level {loyaltyLevel} not found.");

            // Backing field of the "CurrentLoyalty" property
            Traverse.Create(__instance).Field("traderLoyaltyLevel_0").SetValue(loyaltyLevelSettings.Value);
        }
    }
}
