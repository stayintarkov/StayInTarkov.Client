using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_Pickup_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "FCPickup";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "Pickup", false, true);
            return method;
        }


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            )
        {
            return true;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool p)
        {
            //GetLogger(typeof(FirearmController_Pickup_Patch)).LogDebug("PostPatch");

            var player = ____player;

            FCPickupPacket pickupPicket = new(player.ProfileId, p);
            GameClient.SendData(pickupPicket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            FCPickupPacket pp = new(null, false);
            pp.Deserialize((byte[])dict["data"]);

            if (player.HandsController is EFT.Player.FirearmController firearmCont && pp.Pickup)
            {
                firearmCont.CurrentOperation.Pickup(pp.Pickup);
            }
        }

       
    }
}
