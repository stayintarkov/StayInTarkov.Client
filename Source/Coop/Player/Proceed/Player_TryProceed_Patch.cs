using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
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

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            // Giving 'false' to AI and player can cause some major issue!
            // return CallLocally.Contains(__instance.ProfileId) || IsHighPingOrAI(__instance);

            return true;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, Item item, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            Logger.LogInfo("Sending TryProceed Packet");
            PlayerProceedPacket playerProceedPacket = new(__instance.ProfileId, item.Id, item.TemplateId, scheduled, "TryProceed");
            //GameClient.SendData(ref playerProceedPacket);  
            GameClient.SendData(playerProceedPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedPacket playerProceedPacket = new(player.ProfileId, null, null, true, null);
            playerProceedPacket.Deserialize((byte[])dict["data"]);

            if (HasProcessed(GetType(), player, playerProceedPacket))
                return;

            // Prevent compass switch code running twice.
            if (player.IsYourPlayer && playerProceedPacket.TemplateId == "5f4f9eb969cdc30ff33f09db") // EYE MK.2 professional hand-held compass
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
            {
                CallLocally.Add(player.ProfileId);

                // Make sure Tagilla and Cultists are using correct callback.
                if (player.IsAI && item.Attributes.Any(x => x.Name == "knifeDurab"))
                {
                    BotOwner botOwner = player.AIData.BotOwner;
                    if (botOwner != null)
                    {
                        botOwner.WeaponManager.Selector.ChangeToMelee();
                        return;
                    }
                }

                player.TryProceed(item, null, playerProceedPacket.Scheduled);
            }
            else
            {
                Logger.LogError($"Player_TryProceed_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");

                // Prevent softlock the player by switching to empty hands.
                if (player.IsYourPlayer)
                    player.Proceed(true, null, true);
            }

            if (stopwatch.ElapsedMilliseconds > 1)
                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(Player_TryProceed_Patch)} TryFindItem took {stopwatch.ElapsedMilliseconds}ms to process!");
        }
    }
}