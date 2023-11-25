using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    internal class PlayerInventoryController_ThrowItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.PlayerInventoryController);

        public override string MethodName => "ThrowItem";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, "ThrowItem", false, true);

        [PatchPrefix]
        public static bool PrePatch(EFT.Player.PlayerInventoryController __instance, Item item, Profile ___profile_0)
        {
            return CallLocally.Contains(___profile_0.ProfileId);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.PlayerInventoryController __instance, Item item, Profile ___profile_0)
        {
            if (CallLocally.Contains(___profile_0.ProfileId))
            {
                CallLocally.Remove(___profile_0.ProfileId);
                return;
            }

            ItemPlayerPacket itemPacket = new(___profile_0.ProfileId, item.Id, item.TemplateId, "ThrowItem");
            AkiBackendCommunication.Instance.SendDataToPool(itemPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo($"PlayerInventoryController_ThrowItem_Patch.Replicated");

            if (!dict.ContainsKey("data"))
                return;

            ItemPlayerPacket itemPacket = new(null, null, null, null);
            itemPacket = itemPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, itemPacket))
                return;

            if (ItemFinder.TryFindItemController(player.ProfileId, out ItemController itemController))
            {
                if (itemController is EFT.Player.PlayerInventoryController playerInventoryController)
                {
                    if (ItemFinder.TryFindItem(itemPacket.ItemId, out Item item))
                    {
                        CallLocally.Add(player.ProfileId);
                        playerInventoryController.ThrowItem(item, GetDestroyedItemsFromItem(playerInventoryController, item));
                    }
                    else
                    {
                        Logger.LogError($"PlayerInventoryController_ThrowItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
                    }
                }
                else
                {
                    Logger.LogError("PlayerInventoryController_ThrowItem_Patch.Replicated. ItemController doesn't have derived class PlayerInventoryController!");
                }
            }
            else
            {
                Logger.LogError("PlayerInventoryController_ThrowItem_Patch.Replicated. Unable to find Item Controller");
            }
        }

        public static List<ItemsCount> GetDestroyedItemsFromItem(EFT.Player.PlayerInventoryController playerInventoryController, Item item)
        {
            List<ItemsCount> destroyedItems = new();

            if (playerInventoryController.HasDiscardLimits)
            {
                if (item.LimitedDiscard)
                {
                    int itemDiscardLimit = item.DiscardLimit.Value;
                    if (item.StackObjectsCount > itemDiscardLimit)
                        destroyedItems.Add(new ItemsCount(item, item.StackObjectsCount - itemDiscardLimit, itemDiscardLimit));
                }

                if (item.IsContainer && destroyedItems.Count == 0)
                {
                    Item[] itemsInContainer = item.GetAllItems()?.ToArray();
                    if (itemsInContainer != null)
                    {
                        Dictionary<string, int> discardItems = new();

                        for (int i = 0; i < itemsInContainer.Count(); i++)
                        {
                            Item itemInContainer = itemsInContainer[i];
                            if (itemInContainer == item)
                                continue;

                            if (itemInContainer.LimitedDiscard)
                            {
                                int itemInContainerDiscardLimit = itemInContainer.DiscardLimit.Value;

                                if (!destroyedItems.Any(x => x.Item.TemplateId == itemInContainer.TemplateId))
                                {
                                    string templateId = itemInContainer.TemplateId;
                                    if (discardItems.ContainsKey(templateId))
                                        discardItems[templateId] += itemInContainer.StackObjectsCount;
                                    else
                                        discardItems.Add(templateId, itemInContainer.StackObjectsCount);

                                    if (discardItems[templateId] > itemInContainerDiscardLimit)
                                        destroyedItems.Add(new ItemsCount(itemInContainer, discardItems[templateId] - itemInContainerDiscardLimit, itemInContainer.StackObjectsCount - (discardItems[templateId] - itemInContainerDiscardLimit)));
                                }
                                else
                                {
                                    destroyedItems.Add(new ItemsCount(itemInContainer, itemInContainer.StackObjectsCount, 0));
                                }
                            }
                        }
                    }
                }
            }

            return destroyedItems;
        }
    }
}