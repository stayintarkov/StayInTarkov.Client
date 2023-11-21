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
    internal class Player_Proceed_Meds_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedMeds";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "meds");
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            return CallLocally.Contains(__instance.ProfileId) || IsHighPingOrAI(__instance);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, Meds0 meds, EBodyPart bodyPart, int animationVariant, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            PlayerProceedMedsPacket playerProceedMedsPacket = new(__instance.ProfileId, meds.Id, meds.TemplateId, bodyPart, animationVariant, scheduled, "ProceedMeds");
            AkiBackendCommunication.Instance.SendDataToPool(playerProceedMedsPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedMedsPacket playerProceedMedsPacket = new(null, null, null, 0, 0, true, null);
            playerProceedMedsPacket = playerProceedMedsPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, playerProceedMedsPacket))
                return;

            if (ItemFinder.TryFindItem(playerProceedMedsPacket.ItemId, out Item item))
            {
                if (item is Meds0 meds)
                {
                    CallLocally.Add(player.ProfileId);

                    Callback<IHandsController5> callback = null;
                    if (player.IsAI)
                    {
                        BotOwner botOwner = player.AIData.BotOwner;
                        if (botOwner != null)
                        {
                            callback = (IResult) =>
                            {
                                botOwner.WeaponManager.Selector.TakePrevWeapon();
                                botOwner.AITaskManager.RegisterDelayedTask(botOwner, 1f, new Action(botOwner.Medecine.FirstAid.CheckParts));
                            };
                        }
                    }

                    player.Proceed(meds, playerProceedMedsPacket.BodyPart, callback, playerProceedMedsPacket.AnimationVariant, playerProceedMedsPacket.Scheduled);
                }
                else
                {
                    Logger.LogError($"Player_Proceed_Meds_Patch:Replicated. Item {playerProceedMedsPacket.ItemId} is not a Meds0!");
                }
            }
            else
            {
                Logger.LogError($"Player_Proceed_Meds_Patch:Replicated. Cannot found item {playerProceedMedsPacket.ItemId}!");
            }
        }
    }

    public class PlayerProceedMedsPacket : PlayerProceedPacket
    {
        public EBodyPart BodyPart { get; set; }

        public int AnimationVariant { get; set; }

        public PlayerProceedMedsPacket(string profileId, string itemId, string templateId, EBodyPart bodyPart, int animationVariant, bool scheduled, string method) : base(profileId, itemId, templateId, scheduled, method)
        {
            BodyPart = bodyPart;
            AnimationVariant = animationVariant;
        }
    }
}