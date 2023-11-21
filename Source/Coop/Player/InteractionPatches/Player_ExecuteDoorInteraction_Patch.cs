using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.InteractionPatches
{
    internal class Player_ExecuteDoorInteraction_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(MovementState);

        public override string MethodName => "ExecuteDoorInteraction";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName); // EFT.Player.vmethod_1()
        }

        [PatchPrefix]
        public static bool PrePatch(MovementState __instance, WorldInteractiveObject interactive, InteractionResult interactionResult, Action callback, EFT.Player user)
        {
            if (CallLocally.Contains(user.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PatchPostfix(MovementState __instance, WorldInteractiveObject interactive, InteractionResult interactionResult, Action callback, EFT.Player user)
        {
            if (CallLocally.Contains(user.ProfileId))
            {
                CallLocally.Remove(user.ProfileId);
                return;
            }

            Dictionary<string, object> dict = new()
            {
                { "serverId", CoopGameComponent.GetServerId() },
                { "t", DateTime.Now.Ticks.ToString("G") },
                { "m", "ExecuteDoorInteraction" },
                { "profileId", user.ProfileId },
                { "WIOId", interactive.Id },
                { "interactionType", (int)interactionResult.InteractionType }
            };

            if (interactionResult is KeyInteractionResult keyInteractionResult)
            {
                KeyComponent key = keyInteractionResult.Key;

                dict.Add("keyItemId", key.Item.Id);
                dict.Add("keyTemplateId", key.Item.TemplateId);

                if (key.Template.MaximumNumberOfUsage > 0 && key.NumberOfUsages + 1 >= key.Template.MaximumNumberOfUsage)
                    callback();

                ItemAddress itemAddress = keyInteractionResult.DiscardResult != null ? keyInteractionResult.From : key.Item.Parent;
                if (itemAddress is GridItemAddress grid)
                {
                    GridItemAddressDescriptor gridItemAddressDescriptor = new();
                    gridItemAddressDescriptor.Container = new();
                    gridItemAddressDescriptor.Container.ContainerId = grid.Container.ID;
                    gridItemAddressDescriptor.Container.ParentId = grid.Container.ParentItem?.Id;
                    gridItemAddressDescriptor.LocationInGrid = grid.LocationInGrid;
                    dict.Add("keyParentGrid", gridItemAddressDescriptor);
                }

                dict.Add("succeed", keyInteractionResult.Succeed);
            }

            AkiBackendCommunication.Instance.SendDataToPool(dict.ToJson());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("Player_ExecuteDoorInteraction_Patch:Replicated");

            if (HasProcessed(GetType(), player, dict))
                return;

            if (!CoopGameComponent.TryGetCoopGameComponent(out CoopGameComponent coopGameComponent))
                return;

            if (!ItemFinder.TryFindItemController(player.ProfileId, out ItemController itemController))
                return;

            WorldInteractiveObject worldInteractiveObject = coopGameComponent.ListOfInteractiveObjects.FirstOrDefault(x => x.Id == dict["WIOId"].ToString());

            if (worldInteractiveObject == null)
                return;

            InteractionResult interactionResult = new((EInteractionType)int.Parse(dict["interactionType"].ToString()));
            KeyInteractionResult keyInteractionResult = null;

            if (dict.ContainsKey("keyItemId"))
            {
                string itemId = dict["keyItemId"].ToString();
                if (!ItemFinder.TryFindItem(itemId, out Item item))
                    item = Spawners.ItemFactory.CreateItem(itemId, dict["keyTemplateId"].ToString());

                if (item != null)
                {
                    if (item.TryGetItemComponent(out KeyComponent keyComponent))
                    {
                        DiscardResult discardResult = null;

                        if (dict.ContainsKey("keyParentGrid"))
                        {
                            ItemAddress itemAddress = itemController.ToGridItemAddress(dict["keyParentGrid"].ToString().SITParseJson<GridItemAddressDescriptor>());
                            discardResult = new DiscardResult(new RemoveResult(item, itemAddress, itemController, new ResizeResult(item, itemAddress, ItemMovementHandler.ResizeAction.Addition, null, null), null, false), null, null, null);
                        }

                        keyInteractionResult = new KeyInteractionResult(keyComponent, discardResult, bool.Parse(dict["succeed"].ToString()));
                    }
                    else
                    {
                        Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not a KeyComponent object.");
                    }
                }
                else
                {
                    Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not found.");
                }
            }

            CallLocally.Add(player.ProfileId);
            player.CurrentManagedState.ExecuteDoorInteraction(worldInteractiveObject, keyInteractionResult ?? interactionResult, keyInteractionResult == null ? null : () => keyInteractionResult.RaiseEvents(itemController, CommandStatus.Failed), player);
        }
    }
}