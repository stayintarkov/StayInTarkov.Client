using EFT.HealthSystem;
using StayInTarkov.Coop.Players;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Credit SPT-Aki team - https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/Healing/MainMenuControllerPatch.cs
    /// Modified by Paulov to tie in with custom HealthListener
    /// </summary>
    public class MainMenuControllerForHealthListenerPatch : ModulePatch
    {
        static MainMenuControllerForHealthListenerPatch()
        {
            _ = nameof(IHealthController.HydrationChangedEvent);
            _ = nameof(MainMenuController.HealthController);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MainMenuController);
            var desiredMethod = ReflectionHelpers.GetMethodForType(desiredType, "ShowScreen");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance)
        {
            var healthController = __instance.HealthController;
            var listener = HealthListener.Instance;

            if (healthController == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - healthController is null");
            }
            else
            {
                foreach (var p in Object.FindObjectsOfType<CoopPlayer>())
                {
                    Object.Destroy(p);
                }
            }

            if (listener == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - listener is null");
            }

            if (healthController != null && listener != null)
            {
                listener.Init(healthController, false);
            }

            listener.Update(healthController, false);
        }
    }
}
