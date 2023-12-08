using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    public class Player_Proceed_Weapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedWeapon";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "weapon");
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            // Giving 'false' to AI and player can cause some major issue!
            // return CallLocally.Contains(__instance.ProfileId) || IsHighPingOrAI(__instance);

            return true;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, Weapon weapon, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            PlayerProceedPacket playerProceedPacket = new(__instance.ProfileId, weapon.Id, weapon.TemplateId, scheduled, "ProceedWeapon");
            AkiBackendCommunication.Instance.SendDataToPool(playerProceedPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedPacket playerProceedPacket = new(null, null, null, true, null);
            playerProceedPacket = playerProceedPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, playerProceedPacket))
                return;

            if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
            {
                if (item is Weapon weapon)
                {
                    CallLocally.Add(player.ProfileId);
                    player.Proceed(weapon, null, playerProceedPacket.Scheduled);
                }
                else
                {
                    Logger.LogError($"Player_Proceed_Weapon_Patch:Replicated. Item {playerProceedPacket.ItemId} is not a Weapon!");
                }
            }
            else
            {
                Logger.LogError($"Player_Proceed_Weapon_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");
            }
        }
    }
}