using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using StayInTarkov.Coop.Controllers.Health;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health
{
    internal class PHC_HandleFall_Patch : ModulePatch
    {
        public Type InstanceType => typeof(ActiveHealthController);

        public string MethodName => "HandleFall";

        public PHC_HandleFall_Patch()
        {
        }

        private ManualLogSource GetLogger()
        {
            return GetLogger(typeof(PHC_HandleFall_Patch));
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(
            ActiveHealthController __instance
            )
        {
            //Logger.LogInfo($"{nameof(PHC_HandleFall_Patch)}:{__instance.GetType()}");
            if (!SITMatchmaking.IsClient)
                return true;
            
            if (SITMatchmaking.IsClient)
                return __instance.GetType() == typeof(SITHealthController);

            return false;
        }

    }
}
