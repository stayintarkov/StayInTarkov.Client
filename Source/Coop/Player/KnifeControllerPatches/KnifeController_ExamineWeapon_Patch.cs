using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.KnifeControllerPatches
{
    internal class GrenadeController_ExamineWeapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.KnifeController);
        public override string MethodName => "KnifeController_ExamineWeapon";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "ExamineWeapon");
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

            AkiBackendCommunication.Instance.SendDataToPool(new BasePlayerPacket(____player.ProfileId, "KnifeController_ExamineWeapon").Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("KnifeController_ExamineWeapon_Patch:Replicated");

            if (!dict.ContainsKey("data"))
                return;

            BasePlayerPacket examineWeaponPacket = new(player.ProfileId, "");
            examineWeaponPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, examineWeaponPacket))
                return;

            if (player.HandsController is EFT.Player.KnifeController knifeController)
            {
                CallLocally.Add(player.ProfileId);
                knifeController.ExamineWeapon();
            }
        }
    }
}