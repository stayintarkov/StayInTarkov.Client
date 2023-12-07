using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.ItemControllerPatches
{
    internal class Player_ToMoveOperation_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "ToMoveOperation";
        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPostfix]
        public static void PostPatch(ItemController __instance, MoveOperationDescriptor descriptor)
        {
            var coopPlayer = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
            Logger.LogInfo("Player_ToMoveOperation_Patch");

            if (coopPlayer != null)
            {                
                var type = descriptor.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var names = Array.ConvertAll(fields, field => field.Name);
                var values = Array.ConvertAll(fields, field => field.GetValue(descriptor));

                foreach (var name in names)
                {
                    Logger.LogInfo("Name: " + names[names.IndexOf(name)] + " Value: " + values[names.IndexOf(name)]);
                }

                Logger.LogInfo(descriptor.OperationId);

                coopPlayer.AddCommand(new GClass2110()
                {
                    Operation = descriptor
                });
            }
            else
            {
                Logger.LogError("Player_ToMoveOperation_Patch::PostPatch CoopPlayer was null!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }


    }
}
