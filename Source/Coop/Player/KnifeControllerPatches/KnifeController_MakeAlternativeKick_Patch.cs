using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.KnifeControllerPatches
{
    internal class KnifeController_MakeAlternativeKick_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.KnifeController);
        public override string MethodName => "MakeAlternativeKick";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(object __instance, EFT.Player ____player)
        {
            if (CallLocally.Contains(____player.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(object __instance, EFT.Player ____player)
        {
            if (CallLocally.Contains(____player.ProfileId))
            {
                CallLocally.Remove(____player.ProfileId);
                return;
            }

            GameClient.SendDataToServer(new BasePlayerPacket(____player.ProfileId, "MakeAlternativeKick").Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("KnifeController_MakeAlternativeKick_Patch:Replicated");

            if (!dict.ContainsKey("data"))
                return;

            BasePlayerPacket makeAlternativeKickPacket = new(player.ProfileId, "");
            makeAlternativeKickPacket.Deserialize((byte[])dict["data"]);

            if (HasProcessed(GetType(), player, makeAlternativeKickPacket))
                return;

            if (player.HandsController is EFT.Player.KnifeController knifeController)
            {
                CallLocally.Add(player.ProfileId);
                knifeController.MakeAlternativeKick();
            }
        }
    }
}