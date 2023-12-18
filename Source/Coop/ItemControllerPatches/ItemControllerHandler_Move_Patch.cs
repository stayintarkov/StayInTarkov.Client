//using BepInEx.Logging;
//using Comfort.Common;
//using EFT;
//using EFT.InventoryLogic;
//using StayInTarkov.Coop.Matchmaker;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace StayInTarkov.Coop.ItemControllerPatches
//{
//    internal class ItemControllerHandler_Move_Patch : ModuleReplicationPatch, IModuleReplicationWorldPatch
//    {
//        public override Type InstanceType => typeof(ItemMovementHandler);

//        public override string MethodName => "Move";

//        public static HashSet<string> CallLocally = new();

//        public static HashSet<string> DisableForPlayer = new();

//        public static HashSet<string> AlreadySentNoTime = new();
//        public static Dictionary<string, DateTime> LastSendReceiveOfItemId = new();

//        private ManualLogSource GetLogger()
//        {
//            return GetLogger(typeof(ItemControllerHandler_Move_Patch));
//        }

//        public static bool RunLocally = true;

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//        }

//        [PatchPostfix]
//        public static void Postfix(object __instance,Item item, ItemAddress to, ItemController itemController, bool simulate = false)
//        {
//            var player = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
//            if (player == null || !player.IsYourPlayer && !MatchmakerAcceptPatches.IsServer && !player.IsAI)
//                return;

//            if (RunLocally == false)
//            {
//                RunLocally = true;
//                return;
//            }

//            if (to is GridItemAddress gridItemAddress)
//            {
//                GridItemAddressDescriptor gridItemAddressDescriptor = new();
//                gridItemAddressDescriptor.Container = new();
//                gridItemAddressDescriptor.Container.ContainerId = to.Container.ID;
//                gridItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
//                gridItemAddressDescriptor.LocationInGrid = gridItemAddress.LocationInGrid;
//                player.InventoryPacket.HasItemMovementHandlerMovePacket = true;
//                player.InventoryPacket.ItemMovementHandlerMovePacket = new()
//                {
//                    ItemId = item.Id,
//                    Descriptor = gridItemAddressDescriptor
//                };
//                player.InventoryPacket.ToggleSend();
//                return;
//            }

//            if (to is SlotItemAddress slotItemAddress)
//            {
//                SlotItemAddressDescriptor slotItemAddressDescriptor = new();
//                slotItemAddressDescriptor.Container = new();
//                slotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
//                slotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
//                player.InventoryPacket.HasItemMovementHandlerMovePacket = true;
//                player.InventoryPacket.ItemMovementHandlerMovePacket = new()
//                {
//                    ItemId = item.Id,
//                    Descriptor = slotItemAddressDescriptor
//                };
//                player.InventoryPacket.ToggleSend();
//                return;
//            }

//            if (to is StackSlotItemAddress stackSlotItemAddress)
//            {
//                StackSlotItemAddressDescriptor stackSlotItemAddressDescriptor = new();
//                stackSlotItemAddressDescriptor.Container = new();
//                stackSlotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
//                stackSlotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
//                player.InventoryPacket.HasItemMovementHandlerMovePacket = true;
//                player.InventoryPacket.ItemMovementHandlerMovePacket = new()
//                {
//                    ItemId = item.Id,
//                    Descriptor = stackSlotItemAddressDescriptor
//                };
//                player.InventoryPacket.ToggleSend();
//                return;
//            }

//            EFT.UI.ConsoleScreen.LogError("ItemControllerHandler_Move_Patch: There were no descriptors!");
//        }



//        public void Replicated(ref Dictionary<string, object> packet)
//        {
//            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

//            var itemControllerId = packet["icId"].ToString();
//            GetLogger().LogDebug($"Item Controller Id: {itemControllerId}");
//            GetLogger().LogDebug($"Item Controller Current Id: {packet["icCId"]}");

//            var itemId = packet["id"].ToString();
//            if (!ItemFinder.TryFindItem(itemId, out Item item))
//            {
//                GetLogger().LogError("Item not found!");
//                return;
//            }

//            // -----------------------------------------------------------------------------------
//            // Destroy the Loose Item if it exists
//            //
//            var world = Singleton<GameWorld>.Instance;
//            var lootItems = world.LootItems.Where(x => x.ItemId == itemId);

//            if (lootItems.Any())
//            {
//                foreach (var i in lootItems)
//                {
//                    world.LootList.Remove(i);
//                    i.Kill();
//                }
//            }
//            //
//            // -----------------------------------------------------------------------------------

//            //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);


//            try
//            {
//                ItemController itemController = null;
//                ItemAddress address = null;
//                AbstractDescriptor descriptor = null;
//                if (packet.ContainsKey("grad"))
//                {
//                    GetLogger().LogDebug(packet["grad"].ToString());
//                    descriptor = packet["grad"].ToString().SITParseJson<GridItemAddressDescriptor>();
//                }

//                if (packet.ContainsKey("sitad"))
//                {
//                    GetLogger().LogInfo(packet["sitad"].ToString());
//                    descriptor = packet["sitad"].ToString().SITParseJson<SlotItemAddressDescriptor>();

//                }

//                if (packet.ContainsKey("ssad"))
//                {
//                    GetLogger().LogError("ssad has not been handled!");
//                    GetLogger().LogInfo(packet["ssad"].ToString());

//                    descriptor = packet["ssad"].ToString().SITParseJson<StackSlotItemAddressDescriptor>();
//                }

//                if (descriptor == null)
//                {
//                    GetLogger().LogError($"Unable to find Descriptor for {item.Id}");
//                    return;
//                }

//                if (!ItemFinder.TryFindItemController(descriptor.Container.ParentId, out itemController))
//                {
//                    if (!ItemFinder.TryFindItemController(itemControllerId, out itemController))
//                    {
//                        GetLogger().LogError("Unable to find ItemController");
//                        return;
//                    }
//                }

//                if (itemController == null)
//                {
//                    GetLogger().LogError($"Unable to find Item Controller for {item.Id}");
//                    return;
//                }

//                address = itemController.ToItemAddress(descriptor);

//                if (address == null)
//                {
//                    GetLogger().LogError($"Unable to find Address for {item.Id}");
//                    return;
//                }

//                if (CallLocally.Contains(itemControllerId))
//                {
//                    GetLogger().LogError($"CallLocally already contains {itemControllerId}");
//                    return;
//                }

//                if (!LastSendReceiveOfItemId.ContainsKey(item.Id))
//                    LastSendReceiveOfItemId.Add(item.Id, DateTime.Now);
//                else
//                    LastSendReceiveOfItemId[item.Id] = DateTime.Now;

//                CallLocally.Add(itemController.ID);

//                ItemMovementHandler.Move(item, address, itemController, false);

//            }
//            catch (Exception)
//            {

//            }
//            finally
//            {
//                CallLocally.RemoveWhere(x => x != itemControllerId);
//            }

//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
