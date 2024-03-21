using EFT;
using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
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
