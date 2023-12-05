using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.ItemControllerPatches
{
    internal class ItemController_Execute_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(InventoryController);
        public override string MethodName => "Execute";
        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPostfix]
        public static void PostPatch(ItemController __instance, AbstractInternalOperation operation)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
            var observedPlayer = Singleton<GameWorld>.Instance.GetObservedPlayerByProfileID(player.ProfileId);

            Logger.LogInfo("ExecutePatch: " + player + " " + observedPlayer);

            if (player != null)
            {
                var op = GClass1570.FromInventoryOperation(operation, true);
                var type = op.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var names = Array.ConvertAll(fields, field => field.Name);
                var values = Array.ConvertAll(fields, field => field.GetValue(op));

                foreach ( var name in names )
                {
                    Logger.LogInfo("Name: " + names[names.IndexOf(name)] + " Value: " + values[names.IndexOf(name)]);
                }

                Logger.LogInfo(op.OperationId);

                player.AddCommand(new GClass2110()
                {
                    Operation = op
                });
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }


    }
}
