using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_Pickup_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "FCPickup";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "Pickup", false, true);
            return method;
        }

        public static List<string> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            )
        {
            //Logger.LogInfo("FirearmController_Pickup_Patch.PrePatch");

            if (CoopGameComponent.GetCoopGameComponent() == null)
                return false;

            //if (AkiBackendCommunication.Instance.HighPingMode && ____player.IsYourPlayer)
            //{
            //    return true;
            //}

            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool p)
        {
            GetLogger(typeof(FirearmController_Pickup_Patch)).LogInfo("PostPatch");

            var player = ____player;
            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            FCPickupPicket pickupPicket = new(player.ProfileId, p);
            AkiBackendCommunication.Instance.SendDataToPool(pickupPicket.Serialize());



        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("FirearmController_Pickup_Patch.Replicated");

            if (AkiBackendCommunication.Instance.HighPingMode && player.IsYourPlayer)
            {
                // You would have already run this. Don't bother
                return;
            }

            FCPickupPicket pp = new(null, false);
            pp = pp.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, pp))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont && pp.Pickup)
            {
                CallLocally.Add(pp.ProfileId);
                firearmCont.Pickup(pp.Pickup);
            }
        }

        public class FCPickupPicket : BasePlayerPacket
        {
            public bool Pickup { get; set; }

            public FCPickupPicket(string profileId, bool pickup) : base(profileId, "FCPickup")
            {
                Pickup = pickup;
            }
        }
    }
}
