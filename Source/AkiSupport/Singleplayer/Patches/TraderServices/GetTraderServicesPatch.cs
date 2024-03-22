using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.TraderServices
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/TraderServices/GetTraderServicesPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class GetTraderServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryControllerClass), nameof(InventoryControllerClass.GetTraderServicesDataFromServer));
        }

        [PatchPrefix]
        public static bool PatchPrefix(string traderId)
        {
            Logger.LogInfo($"Loading {traderId} services from servers");
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(traderId);

            // Skip original
            return false;
        }
    }
}
