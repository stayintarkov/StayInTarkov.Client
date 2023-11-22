using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_CheckChamber_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "CheckChamber";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            if (CallLocally.Contains(player.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance)
        {
            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            AkiBackendCommunication.Instance.SendDataToPool(new BasePlayerPacket(player.ProfileId, "CheckChamber").Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("FirearmController_CheckChamber_Patch:Replicated");

            BasePlayerPacket checkChamberPacket = new();

            if (dict.ContainsKey("data"))
                checkChamberPacket = checkChamberPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, checkChamberPacket))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.ProfileId);
                firearmCont.CheckChamber();
            }
        }
    }
}
