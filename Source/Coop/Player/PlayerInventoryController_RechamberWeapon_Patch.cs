using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Player
{
    internal class PlayerInventoryController_RechamberWeapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(InventoryControllerClass);

        public override string MethodName => "RechamberWeapon";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "RechamberWeapon", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(InventoryControllerClass __instance, Weapon weapon)
        {
            Logger.LogInfo("PlayerInventoryController_RechamberWeapon_Patch:PrePatch");

            if (CallLocally.Contains(__instance.Profile.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(InventoryControllerClass __instance, Weapon weapon)
        {
            Logger.LogInfo("PlayerInventoryController_RechamberWeapon_Patch:PostPatch");

            if (CallLocally.Contains(__instance.Profile.ProfileId))
            {
                CallLocally.Remove(__instance.Profile.ProfileId);
                return;
            }

            ItemPlayerPacket itemPacket = new(__instance.Profile.ProfileId, weapon.Id, weapon.TemplateId, "RechamberWeapon");
            var serialized = itemPacket.Serialize();
            GameClient.SendData(serialized);
        }
    }
}