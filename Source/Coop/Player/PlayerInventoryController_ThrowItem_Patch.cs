using EFT;
using EFT.InventoryLogic;
using EFT.UI;
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
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_ThrowItem";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "ThrowItem", false, true);
            return method;
        }

        public static MethodInfo GetFullLocalizedDescriptionMI = null;

        public override void Enable()
        {
            base.Enable();
            if (GetFullLocalizedDescriptionMI == null)
                ReflectionHelpers.GetTypeAndMethodWhereMethodExists("GetFullLocalizedDescription", out _, out GetFullLocalizedDescriptionMI);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player.PlayerInventoryController __instance, Item item, Profile ___profile_0)
        {
            if (CallLocally.Contains(___profile_0.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static async void PostPatch(EFT.Player.PlayerInventoryController __instance, Item item, Profile ___profile_0)
        {
            Logger.LogInfo("PlayerInventoryController_ThrowItem_Patch:PostPatch");

            if (CallLocally.Contains(___profile_0.ProfileId))
            {
                CallLocally.Remove(___profile_0.ProfileId);
                return;
            }

            List<ItemsCount> destroyedItems = GetDestroyedItemsFromItem(__instance, item);
            if (destroyedItems.Count != 0)
            {
                Logger.LogDebug($"PlayerInventoryController_ThrowItem_Patch.PostPatch. Found {destroyedItems.Count} item(s) has hit LimitedDiscard.");

                if (GetFullLocalizedDescriptionMI != null)
                {
                    if (!await ItemUiContext.Instance.ShowScrolledMessageWindow(out _, (string)GetFullLocalizedDescriptionMI.Invoke(null, new object[] { destroyedItems }), "InventoryWarning/ItemsToBeDestroyed".Localized(), true))
                    {
                        Logger.LogWarning($"PlayerInventoryController_ThrowItem_Patch.PostPatch. The player doesn't agree to destroy their LimitedDiscard item(s), ThrowItem failed.");
                        return;
                    }
                }
            }

            ItemPlayerPacket itemPacket = new(___profile_0.ProfileId, item.Id, item.TemplateId, "PlayerInventoryController_ThrowItem");
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
                if (ItemFinder.TryFindItem(itemPacket.ItemId, out Item item))
                {
                    EFT.Player.PlayerInventoryController playerInventoryController = itemController as EFT.Player.PlayerInventoryController;
                    if (playerInventoryController == null)
                        playerInventoryController = new(player, player.Profile, false);

                    List<ItemsCount> destroyedItems = GetDestroyedItemsFromItem(playerInventoryController, item);
                    if (destroyedItems.Count != 0)
                    {
                        if (destroyedItems[0].Item == item && destroyedItems[0].NumberToPreserve == 0)
                        {
                            Logger.LogWarning($"PlayerInventoryController_ThrowItem_Patch.Replicated. The item {itemPacket.TemplateId} cannot be thrown, destroyed.");
                            playerInventoryController.DestroyItem(item);
                            return;
                        }
                    }

                    CallLocally.Add(player.ProfileId);
                    playerInventoryController.ThrowItem(item, destroyedItems);
                }
                else
                {
                    Logger.LogError($"PlayerInventoryController_ThrowItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
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

            //if (playerInventoryController.HasDiscardLimit(item, out int itemDiscardLimit) && item.StackObjectsCount > itemDiscardLimit)
            if (playerInventoryController.HasDiscardLimits && item.LimitedDiscard)
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

                        //if (playerInventoryController.HasDiscardLimit(itemInContainer, out int itemInContainerDiscardLimit))
                        if (playerInventoryController.HasDiscardLimits && itemInContainer.LimitedDiscard)
                        {
                            int itemInContainerDiscardLimit = itemInContainer.DiscardLimit.Value;

                            if (!destroyedItems.Any(x => x.Item.TemplateId == itemInContainer.TemplateId))
                            {
                                string templateId = itemInContainer.TemplateId;
                                if (discardItems.ContainsKey(templateId))
                                {
                                    discardItems[templateId] += itemInContainer.StackObjectsCount;
                                }
                                else
                                {
                                    discardItems.Add(templateId, itemInContainer.StackObjectsCount);
                                }

                                if (discardItems[templateId] > itemInContainerDiscardLimit)
                                {
                                    destroyedItems.Add(new ItemsCount(itemInContainer, discardItems[templateId] - itemInContainerDiscardLimit, itemInContainer.StackObjectsCount - (discardItems[templateId] - itemInContainerDiscardLimit)));
                                }
                            }
                            else
                            {
                                destroyedItems.Add(new ItemsCount(itemInContainer, itemInContainer.StackObjectsCount, 0));
                            }
                        }
                    }
                }
            }

            return destroyedItems;
        }
    }
}