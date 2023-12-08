﻿using BepInEx.Logging;
using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    public class PlayerInventoryController_UnloadMagazine_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_UnloadMagazine";

        public static List<string> CallLocally = new();

        public static HashSet<string> AlreadySent = new();

        private ManualLogSource GetLogger()
        {
            return GetLogger(typeof(PlayerInventoryController_UnloadMagazine_Patch));
        }

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "UnloadMagazine", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch()
        {
            return true;
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //GetLogger(typeof(PlayerInventoryController_UnloadMagazine_Patch)).LogInfo("Replicated");

            ItemPlayerPacket itemPacket = new(null, null, null, null);

            if (dict.ContainsKey("data"))
            {
                itemPacket = itemPacket.DeserializePacketSIT(dict["data"].ToString());
            }
            else
            {
                GetLogger().LogError("Packet did not have data in the dictionary");
                return;
            }

            //if (HasProcessed(GetType(), player, itemPacket))
            //    return;

            if (!ItemFinder.TryFindItemController(player.ProfileId, out var itemController))
            {
                GetLogger().LogError("Unable to find itemController");
                return;
            }

            var inventoryController = itemController as ICoopInventoryController;
            if (inventoryController != null)
            {

                inventoryController.ReceiveUnloadMagazineFromServer(itemPacket);
                //    if (ItemFinder.TryFindItem(itemPacket.MagazineId, out Item magazine))
                //    {
                //        ItemControllerHandler_Move_Patch.DisableForPlayer.Add(player.ProfileId);

                //        CallLocally.Add(player.ProfileId);
                //        //GetLogger(typeof(PlayerInventoryController_UnloadMagazine_Patch)).LogDebug($"Replicated. Calling UnloadMagazine ({magazine.Id})");
                //        inventoryController.ReceiveUnloadMagazineFromServer((MagazineClass)magazine);

                //        ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(player.ProfileId);

                //    }
                //    else
                //    {
                //        GetLogger().LogError($"PlayerInventoryController_UnloadMagazine.Replicated. Unable to find Inventory Controller item {itemPacket.MagazineId}");
                //    }

                //}
                //else
                //{
                //    GetLogger().LogError("PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller object");
                //}
            }
            else
            {
                GetLogger().LogError("Replicated. Unable to find Inventory Controller object");
            }

        }

    }
}
