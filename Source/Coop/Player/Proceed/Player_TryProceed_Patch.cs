using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_TryProceed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "TryProceed";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, Item item, bool scheduled)
        {
            // Do send on clients
            if (__instance is CoopPlayerClient)
                return;

            PlayerTryProceedPacket tryProceedPacket = new PlayerTryProceedPacket(__instance.ProfileId, item, scheduled);
            GameClient.SendData(tryProceedPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            // Do not replicate on local players, only clients
            //if (player is CoopPlayer)
            //    return;

            //if (!dict.ContainsKey("data"))
            //    return;

            //PlayerProceedPacket playerProceedPacket = new(player.ProfileId, null, null, true, null);
            //playerProceedPacket.Deserialize((byte[])dict["data"]);

            //if (HasProcessed(GetType(), player, playerProceedPacket))
            //    return;

            //// Prevent compass switch code running twice.
            //if (player.IsYourPlayer && playerProceedPacket.TemplateId == "5f4f9eb969cdc30ff33f09db") // EYE MK.2 professional hand-held compass
            //    return;

            //Stopwatch stopwatch = Stopwatch.StartNew();

            //if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
            //{
            //    // Make sure Tagilla and Cultists are using correct callback.
            //    if (player.IsAI && item.Attributes.Any(x => x.Name == "knifeDurab"))
            //    {
            //        BotOwner botOwner = player.AIData.BotOwner;
            //        if (botOwner != null)
            //        {
            //            botOwner.WeaponManager.Selector.ChangeToMelee();
            //            return;
            //        }
            //    }

            //    player.TryProceed(item, null, playerProceedPacket.Scheduled);
            //}
            //else
            //{
            //    Logger.LogError($"Player_TryProceed_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");

            //    // Prevent softlock the player by switching to empty hands.
            //    if (player.IsYourPlayer)
            //        player.Proceed(true, null, true);
            //}

            //if (stopwatch.ElapsedMilliseconds > 1)
            //    StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(Player_TryProceed_Patch)} TryFindItem took {stopwatch.ElapsedMilliseconds}ms to process!");
        }
    }
}