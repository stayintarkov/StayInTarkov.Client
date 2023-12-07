using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.ItemControllerPatches
{
    internal class ItemController_Execute_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ItemController);
        public override string MethodName => "Execute";
        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPostfix]
        public static void PostPatch(ItemController __instance, AbstractInternalOperation operation)
        {
            var coopPlayer = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
            Logger.LogInfo("ItemController_Execute_Patch");

            if (coopPlayer != null)
            {
                var op = GClass1570.FromInventoryOperation(operation, true);

                Logger.LogInfo("ItemController_Execute_Patch ToString: " + op.ToString());

                coopPlayer.AddCommand(new GClass2110()
                {
                    Operation = op
                });
            }
            else
            {
                Logger.LogError("ItemController_Execute_Patch::PostPatch CoopPlayer was null!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }


    }
}
