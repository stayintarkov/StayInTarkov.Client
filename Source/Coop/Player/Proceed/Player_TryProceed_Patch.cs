//using EFT;
//using EFT.InventoryLogic;
//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.Proceed
//{
//    internal class Player_TryProceed_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player);

//        public override string MethodName => "TryProceed";

//        public static List<string> CallLocally = new();

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//        }

//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player __instance)
//        {
//            // Giving 'false' to AI and player can cause some major issue!
//            // return CallLocally.Contains(__instance.ProfileId) || IsHighPingOrAI(__instance);

//            return true;
//        }

//        [PatchPostfix]
//        public static void PostPatch(EFT.Player __instance, Item item, bool scheduled)
//        {
//            if (CallLocally.Contains(__instance.ProfileId))
//            {
//                CallLocally.Remove(__instance.ProfileId);
//                return;
//            }

//            PlayerProceedPacket playerProceedPacket = new(__instance.ProfileId, item.Id, item.TemplateId, scheduled, "TryProceed");
//            AkiBackendCommunication.Instance.SendDataToPool(playerProceedPacket.Serialize());
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (!dict.ContainsKey("data"))
//                return;

//            PlayerProceedPacket playerProceedPacket = new(null, null, null, true, null);
//            playerProceedPacket = playerProceedPacket.DeserializePacketSIT(dict["data"].ToString());

//            if (HasProcessed(GetType(), player, playerProceedPacket))
//                return;

//            Stopwatch stopwatch = Stopwatch.StartNew();

//            if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
//            {
//                CallLocally.Add(player.ProfileId);

//                // Make sure Tagilla and Cultists are using correct callback.
//                if (player.IsAI && item is Knife0)
//                {
//                    BotOwner botOwner = player.AIData.BotOwner;
//                    if (botOwner != null)
//                    {
//                        botOwner.WeaponManager.Selector.ChangeToMelee();
//                        return;
//                    }
//                }

//                player.TryProceed(item, null, playerProceedPacket.Scheduled);
//            }
//            else
//            {
//                Logger.LogError($"Player_TryProceed_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");

//                // Prevent softlock the player by switching to empty hands.
//                if (player.IsYourPlayer)
//                    player.Proceed(true, null, true);
//            }

//            if (stopwatch.ElapsedMilliseconds > 1)
//                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(Player_TryProceed_Patch)} TryFindItem took {stopwatch.ElapsedMilliseconds}ms to process!");
//        }
//    }
//}