//using EFT.InventoryLogic;
//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace StayInTarkov.Coop.Player
//{
//    internal class PlayerInventoryController_RechamberWeapon_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(InventoryController);

//        public override string MethodName => "RechamberWeapon";

//        public static List<string> CallLocally = new();

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = ReflectionHelpers.GetMethodForType(InstanceType, "RechamberWeapon", false, true);
//            return method;
//        }

//        [PatchPrefix]
//        public static bool PrePatch(InventoryController __instance, Weapon weapon)
//        {
//            Logger.LogInfo("PlayerInventoryController_RechamberWeapon_Patch:PrePatch");

//            if (CallLocally.Contains(__instance.Profile.ProfileId))
//                return true;

//            return false;
//        }

//        [PatchPostfix]
//        public static void PostPatch(InventoryController __instance, Weapon weapon)
//        {
//            Logger.LogInfo("PlayerInventoryController_RechamberWeapon_Patch:PostPatch");

//            if (CallLocally.Contains(__instance.Profile.ProfileId))
//            {
//                CallLocally.Remove(__instance.Profile.ProfileId);
//                return;
//            }

//            ItemPlayerPacket itemPacket = new(__instance.Profile.ProfileId, weapon.Id, weapon.TemplateId, "RechamberWeapon");
//            var serialized = itemPacket.Serialize();
//            AkiBackendCommunication.Instance.SendDataToPool(serialized);
//        }

//        public override async void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            Logger.LogInfo($"PlayerInventoryController_RechamberWeapon_Patch.Replicated");

//            if (!dict.ContainsKey("data"))
//                return;

//            ItemPlayerPacket itemPacket = new(null, null, null, "");
//            itemPacket = itemPacket.DeserializePacketSIT(dict["data"].ToString());

//            if (HasProcessed(GetType(), player, itemPacket))
//                return;

//            if (player.IsYourPlayer)
//            {
//                if (ItemFinder.TryFindItemController(player.ProfileId, out ItemController itemController))
//                {
//                    if (ItemFinder.TryFindItem(itemPacket.ItemId, out Item item))
//                    {
//                        if (item is Weapon weapon)
//                        {
//                            CallLocally.Add(player.ProfileId);
//                            Logger.LogInfo($"PlayerInventoryController_RechamberWeapon_Patch.Replicated. Calling RechamberWeapon ({itemPacket.ItemId})");
//                            itemController.RechamberWeapon(weapon);
//                        }
//                    }
//                    else
//                    {
//                        Logger.LogError($"PlayerInventoryController_RechamberWeapon_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
//                    }
//                }
//                else
//                {
//                    Logger.LogError("PlayerInventoryController_RechamberWeapon_Patch.Replicated. Unable to find Item Controller");
//                }
//            }
//            else
//            {
//                var firearmsAnimator = player.HandsController.FirearmsAnimator;
//                if (firearmsAnimator != null)
//                {
//                    firearmsAnimator.Rechamber(true);
//                    await Task.Delay(250);
//                    firearmsAnimator.Rechamber(false);
//                }
//            }
//        }
//    }
//}