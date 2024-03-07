using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_TryProceed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "TryProceed";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, Item item, bool scheduled)
        {
            // Do send on clients
            if (__instance is CoopPlayerClient)
                return;

            PlayerTryProceedPacket tryProceedPacket = new PlayerTryProceedPacket(__instance.ProfileId, item, scheduled);
            GameClient.SendData(tryProceedPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            // Leave empty. Processed via the Packet itself.
        }
    }
}