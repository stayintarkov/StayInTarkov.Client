using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers.CoopInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop
{
    public static class ItemFinder
    {
        private static ManualLogSource Logger;

        static ItemFinder()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ItemFinder));
        }

        public static bool TryFindItemOnPlayer(EFT.Player player, string templateId, string itemId, out Item item)
        {
            item = null;

            var result = player.FindItemById(itemId, false, false);
            if (result.Succeeded)
            {
                item = result.Value;
                if(item.Owner == null)
                {
                    Logger.LogError($"{nameof(TryFindItemOnPlayer)} item owner is null!");
                }
            }
            else
            {
                Logger.LogError($"{nameof(TryFindItemOnPlayer)} {result.Error}");
            }

            //if (!string.IsNullOrEmpty(templateId))
            //{
            //    //var allItemsOfTemplate = player.Profile.Inventory.GetAllItemByTemplate(templateId);
            //    var allEquipmentItems = player.Profile.Inventory.GetPlayerItems(EPlayerItems.Equipment);

            //    if (!allEquipmentItems.Any())
            //        return false;

            //    item = allEquipmentItems.FirstOrDefault(x => x.Id == itemId);
            //}
            //else
            //{
            //    item = player.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.Id == itemId);
            //}
            return item != null;
        }

        public static bool TryFindItemInWorld(string itemId, out EFT.InventoryLogic.Item item)
        {
            item = null;

            var itemFindResult = Singleton<GameWorld>.Instance.FindItemById(itemId);
            if (itemFindResult.Succeeded)
            {
                item = itemFindResult.Value;
            }

            return item != null;

        }

        public static bool TryFindItem(string itemId, out EFT.InventoryLogic.Item item)
        {
            var coopGC = SITGameComponent.GetCoopGameComponent();
            foreach (var player in coopGC.Players)
            {
                if (TryFindItemOnPlayer(player.Value, null, itemId, out item))
                    return item != null;
            }

            if (TryFindItemInWorld(itemId, out item))
                return item != null;

#if DEBUG
            Logger.LogDebug($"{nameof(TryFindItem)}: Unable to find item {itemId}");
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            Logger.LogDebug($"{nameof(TryFindItem)}: {t}");
#endif

            return false;
        }

        public static IEnumerable<object> GetItemComponentsInChildren(Item item, Type componentType)
        {
            MethodInfo method = ReflectionHelpers.GetMethodForType(item.GetType(), "GetItemComponentsInChildren");
            MethodInfo generic = method.MakeGenericMethod(componentType);
            var itemComponent = (IEnumerable<object>)generic.Invoke(item, null);
            return itemComponent;
        }

        public static TraderControllerClass GetPlayerInventoryController(EFT.Player player)
        {
            var inventoryController = ReflectionHelpers.GetFieldFromTypeByFieldType(player.GetType(), typeof(InventoryControllerClass)).GetValue(player) as TraderControllerClass;
            return inventoryController;
        }

        public static bool TryFindItemController(string controllerId, out TraderControllerClass itemController)
        {
            // Find in World
            itemController = Singleton<GameWorld>.Instance.FindControllerById(controllerId);
            if (itemController != null)
                return true;

            // Find a Player
            var coopGC = SITGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return false;

            if (!coopGC.Players.ContainsKey(controllerId))
                return false;

            itemController = GetPlayerInventoryController(coopGC.Players[controllerId]);

            return itemController != null;

        }
    }
}
