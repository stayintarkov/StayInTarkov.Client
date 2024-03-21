using System.Reflection;
using EFT;
using HarmonyLib;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix
{
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