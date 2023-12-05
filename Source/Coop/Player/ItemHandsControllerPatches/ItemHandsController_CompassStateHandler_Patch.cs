using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.ItemHandsControllerPatches
{
    internal class ItemHandsController_CompassStateHandler_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.ItemHandsController);
        public override string MethodName => "CompassStateHandler";

        [PatchPostfix]
        public static void Postfix(EFT.Player.ItemHandsController __instance, EFT.Player ____player, bool isActive)
        {
            var coopPlayer = ____player as CoopPlayer;

            if (coopPlayer != null)
            {
                coopPlayer.AddCommand(new CommandMessage()
                {
                    IsActive = isActive
                });
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }
    }
}
