using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_Pickup_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "Pickup";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName, findFirst: true);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, ref bool p, EFT.Player ____player)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                coopPlayer.AddCommand(new GClass2115()
                {
                    Pickup = p
                });
            }
            else
            {
                Logger.LogError("No CoopPlayer found!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }
    }
}
