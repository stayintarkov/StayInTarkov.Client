using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_CheckAmmo_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "CheckAmmo";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static List<string> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;


            //Logger.LogInfo($"CheckAmmo_prefix:{result}");


            return result;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            //Logger.LogInfo($"CheckAmmo_postfix");

            var player = ____player;
            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            var checkAmmo = new BasePlayerPacket(player.ProfileId, "CheckAmmo").Serialize();
            //Logger.LogInfo(Encoding.UTF8.GetString(checkAmmo));
            AkiBackendCommunication.Instance.SendDataToPool(checkAmmo);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            BasePlayerPacket checkAmmoPacket = new();

            if (dict.ContainsKey("data"))
                checkAmmoPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, checkAmmoPacket))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.ProfileId);
                firearmCont.CheckAmmo();
            }
        }
    }
}
