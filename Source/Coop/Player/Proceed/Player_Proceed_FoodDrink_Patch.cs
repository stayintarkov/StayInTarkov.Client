using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_Proceed_FoodDrink_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedFoodDrink";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "foodDrink");
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            return CallLocally.Contains(__instance.ProfileId) || IsHighPingOrAI(__instance);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, FoodDrink foodDrink, float amount, int animationVariant, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            PlayerProceedFoodDrinkPacket playerProceedFoodDrinkPacket = new(__instance.ProfileId, foodDrink.Id, foodDrink.TemplateId, amount, animationVariant, scheduled, "ProceedFoodDrink");
            GameClient.SendDataToServer(playerProceedFoodDrinkPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedFoodDrinkPacket playerProceedFoodDrinkPacket = new(player.ProfileId, null, null, 0, 0, true, null);
            playerProceedFoodDrinkPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, playerProceedFoodDrinkPacket))
                return;

            if (ItemFinder.TryFindItem(playerProceedFoodDrinkPacket.ItemId, out Item item))
            {
                if (item is FoodDrink foodDrink)
                {
                    CallLocally.Add(player.ProfileId);

                    Callback<IMedsController> callback = null;
                    if (player.IsAI)
                    {
                        BotOwner botOwner = player.AIData.BotOwner;
                        if (botOwner != null)
                        {
                            callback = (IResult) =>
                            {
                                if (IResult.Succeed)
                                {
                                    IResult.Value.SetOnUsedCallback((_) =>
                                    {
                                        botOwner.WeaponManager.Selector.TakePrevWeapon();

                                        botOwner.EatDrinkData.Using = false;
                                        botOwner.EatDrinkData.Activate();
                                    });
                                }
                            };
                        }
                    }

                    player.Proceed(foodDrink, playerProceedFoodDrinkPacket.Amount, callback, playerProceedFoodDrinkPacket.AnimationVariant, playerProceedFoodDrinkPacket.Scheduled);
                }
                else
                {
                    Logger.LogError($"Player_Proceed_FoodDrink_Patch:Replicated. Item {playerProceedFoodDrinkPacket.ItemId} is not a FoodDrink!");
                }
            }
            else
            {
                Logger.LogError($"Player_Proceed_FoodDrink_Patch:Replicated. Cannot found item {playerProceedFoodDrinkPacket.ItemId}!");
            }
        }
    }

    public class PlayerProceedFoodDrinkPacket : PlayerProceedPacket
    {
        public float Amount { get; set; }

        public int AnimationVariant { get; set; }

        public PlayerProceedFoodDrinkPacket(string profileId, string itemId, string templateId, float amount, int animationVariant, bool scheduled, string method) : base(profileId, itemId, templateId, scheduled, method)
        {
            Amount = amount;
            AnimationVariant = animationVariant;
        }
    }
}