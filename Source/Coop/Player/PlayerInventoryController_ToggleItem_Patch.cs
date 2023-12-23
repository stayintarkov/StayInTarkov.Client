using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    internal class PlayerInventoryController_ToggleItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_ToggleItem";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "ToggleItem", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(object __instance, TogglableComponent togglable, Profile ___profile_0)
        {
            Logger.LogDebug("PlayerInventoryController_ToggleItem_Patch:PrePatch");
            return false;
        }

        [PatchPostfix]
        public static void PostPatch(object __instance, TogglableComponent togglable, Profile ___profile_0)
        {
            Logger.LogDebug("PlayerInventoryController_ToggleItem_Patch:PostPatch");

            ItemPlayerPacket itemPacket = new(___profile_0.ProfileId, togglable.Item.Id, togglable.Item.TemplateId, "PlayerInventoryController_ToggleItem");
            var serialized = itemPacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogDebug("PlayerInventoryController_ToggleItem_Patch:Replicated");

            if (!dict.ContainsKey("data"))
                return;

            ItemPlayerPacket itemPacket = new(player.ProfileId, null, null, dict["m"].ToString());
            itemPacket = itemPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, itemPacket))
                return;

            if (ItemFinder.TryFindItem(itemPacket.ItemId, out Item item))
            {
                if (item.TryGetItemComponent(out TogglableComponent togglableComponent))
                {
                    Logger.LogInfo($"PlayerInventoryController_ToggleItem_Patch.Replicated. Calling ToggleItem ({itemPacket.ItemId})");
                    togglableComponent.Toggle();
                    togglableComponent.Item.RaiseRefreshEvent(true);

                    if (player.StateIsSuitableForHandInput)
                    {
                        int animationId = item.GetItemComponent<FaceShieldComponent>() == null ? 20 : 400;
                        if (togglableComponent.On)
                            animationId++;

                        player.SendHandsInteractionStateChanged(true, animationId);
                        player.HandsController.Interact(true, animationId);
                    }
                }
                else
                {
                    Logger.LogError($"PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find TogglableComponent of {itemPacket.ItemId}");
                }
            }
            else
            {
                Logger.LogError($"PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
            }
        }
    }
}