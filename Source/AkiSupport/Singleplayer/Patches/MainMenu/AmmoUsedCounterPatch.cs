using EFT;
using HarmonyLib;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.MainMenu
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/MainMenu/AmmoUsedCounterPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class AmmoUsedCounterPatch : ModulePatch
    {
        private static Player player;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                __instance.Profile.EftStats.SessionCounters.AddLong(1L, ASessionCounterManager.AmmoUsed);
            }
        }
    }
}
