using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StayInTarkov.Coop.ItemControllerPatches
{
    internal class ItemController_Execute_Patch : ModuleReplicationPatch, IModuleReplicationWorldPatch
    {
        public override Type InstanceType => typeof(ItemController);

        public override string MethodName => "Execute";

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }

        public static bool RunLocally = true;

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void Postfix(ItemController __instance, AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
            if (player == null || !player.IsYourPlayer && !MatchmakerAcceptPatches.IsServer && !player.IsAI)
                return;

            if (RunLocally == false)
            {
                RunLocally = true;
                return;
            }

            player.InventoryPacket.HasItemControllerExecutePacket = true;

            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                binaryWriter.WritePolymorph(OperationToDescriptorHelpers.FromInventoryOperation(operation, false, false));
                var opBytes = memoryStream.ToArray();
                player.InventoryPacket.ItemControllerExecutePacket = new()
                {
                    CallbackId = operation.Id,
                    OperationBytesLength = opBytes.Length,
                    OperationBytes = opBytes,
                    InventoryId = __instance.ID
                };
                EFT.UI.ConsoleScreen.Log($"Operation: {operation.GetType().Name}, IC Name: {__instance.Name}");
            }
            player.InventoryPacket.ToggleSend();
        }

        public void Replicated(ref Dictionary<string, object> packet)
        {

        }
    }
}
