using EFT.InventoryLogic;
using SIT.Core.Coop;
using SIT.Core.Coop.Player;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_DropBackpack_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "DropBackpack";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPostfix]
        public static void PatchPostfix(EFT.Player __instance)
        {
            Item backpack = __instance.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack).ContainedItem;
            if (backpack == null)
                return;

            EFT.Player.ItemHandsController itemHandsController = __instance.HandsController as EFT.Player.ItemHandsController;
            if (itemHandsController != null && itemHandsController.CurrentCompassState)
            {
                itemHandsController.SetCompassState(false);
                return;
            }

            if (!ItemFinder.TryFindItemController(__instance.ProfileId, out ItemController itemController))
                return;

            if (__instance.MovementContext.StationaryWeapon != null)
                return;

            if (!itemController.CanThrow(backpack))
                return;

            if (__instance.HandsController.IsPlacingBeacon())
                return;

            if (__instance.HandsController.IsInInteractionStrictCheck())
                return;

            if (__instance.CurrentStateName == EPlayerState.BreachDoor)
                return;

            if (__instance.IsSprintEnabled)
                return;

            EFT.Player.PlayerInventoryController playerInventoryController = itemController as EFT.Player.PlayerInventoryController;
            if (playerInventoryController == null)
            {
                playerInventoryController = new(__instance, __instance.Profile, false);
                playerInventoryController.ResetDiscardLimits();
            }

            if (PlayerInventoryController_ThrowItem_Patch.CallLocally.Contains(__instance.ProfileId))
                PlayerInventoryController_ThrowItem_Patch.CallLocally.Remove(__instance.ProfileId);

            PlayerInventoryController_ThrowItem_Patch.PostPatch(playerInventoryController, backpack, __instance.Profile);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
        }
    }
}
