using System.Reflection;
using EFT;
using HarmonyLib;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/RaidFix/VoIPTogglerPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class VoIPTogglerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ForceMuteVoIPToggler), nameof(ForceMuteVoIPToggler.Awake));
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }
}