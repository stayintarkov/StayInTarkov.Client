using EFT;
using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.TraderServices
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/TraderServices/PurchaseTraderServicePatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class PurchaseTraderServicePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryControllerClass), nameof(InventoryControllerClass.TryPurchaseTraderService));
        }

        [PatchPostfix]
        public static async void PatchPostFix(Task<bool> __result, ETraderServiceType serviceType, AbstractQuestControllerClass questController, string subServiceId)
        {
            bool purchased = await __result;
            if (purchased)
            {
                Logger.LogInfo($"Player purchased service {serviceType}");
                TraderServicesManager.Instance.AfterPurchaseTraderService(serviceType, questController, subServiceId);
            }
            else
            {
                Logger.LogInfo($"Player failed to purchase service {serviceType}");
            }
        }
    }
}
