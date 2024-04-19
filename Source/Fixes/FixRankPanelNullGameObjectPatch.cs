using EFT.UI;
using HarmonyLib;
using System.Reflection;

namespace StayInTarkov.Fixes
{
    /// <summary>
    /// Fix black screen at the end of raid
    /// </summary>
    public class FixRankPanelNullGameObjectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RankPanel), nameof(RankPanel.Show));
        }

        [PatchPrefix]
        protected static bool PatchPrefix(RankPanel __instance)
        {
            if (__instance.gameObject == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"RankPanel.gameObject was null! {new System.Diagnostics.StackTrace()}");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
