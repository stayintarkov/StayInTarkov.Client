using EFT;
using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.Custom/Patches/ResetTraderServicesPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class ResetTraderServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseLocalGame<GamePlayerOwner>), nameof(BaseLocalGame<GamePlayerOwner>.Stop));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            TraderServicesManager.Instance.Clear();
        }
    }
}
