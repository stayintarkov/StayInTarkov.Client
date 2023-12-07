using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop
{
    internal class ItemMovementHandler_EquipItemInSlot_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ItemMovementHandler);
        public override string MethodName => "EquipItemInSlot";
        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPostfix]
        public static void PostPatch(Slot slot, Item item, InventoryController inventoryController, bool simulate)
        {
            var coopPlayer = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;

            Logger.LogInfo("ASDASD: " + slot.Name);

            //if (coopPlayer != null)
            //{ 

            ////    var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(item);

            ////    coopPlayer.AddCommand(new ChangeEquipCommandMessaged()
            ////    {
            ////        IsInInventory = true,
            ////        ItemsForEquip = Components,
            ////        OperationType = ChangeEquipCommandMessaged.EOperationType.Equip,
            ////        SlotType = slot.Name
            ////    });
            ////}
            //else
            //{
            //    Logger.LogError("ItemMovementHandler_EquipItemInSlot_Patch::PostPatch CoopPlayer was null!");
            //}
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }
    }
}
