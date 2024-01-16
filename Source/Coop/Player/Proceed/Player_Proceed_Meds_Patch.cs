using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static void PostPatch(EFT.Player __instance, Meds meds, EBodyPart bodyPart, int animationVariant, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            PlayerProceedMedsPacket playerProceedMedsPacket = new(__instance.ProfileId, meds.Id, meds.TemplateId, bodyPart, animationVariant, scheduled, "ProceedMeds");

            if (__instance.IsAI)
            {
                BotOwner botOwner = __instance.AIData.BotOwner;
                if (botOwner != null)
                {
                    if (botOwner.Medecine.FirstAid.Using)
                        playerProceedMedsPacket.AIMedicineType = "FirstAid";
                    else if (botOwner.Medecine.SurgicalKit.Using)
                        playerProceedMedsPacket.AIMedicineType = "SurgicalKit";
                    else if (botOwner.Medecine.Stimulators.Using)
                        playerProceedMedsPacket.AIMedicineType = "Stimulators";
                }
            }

            GameClient.SendDataToServer(playerProceedMedsPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedMedsPacket playerProceedMedsPacket = new(player.ProfileId, null, null, 0, 0, true, null);
            playerProceedMedsPacket.Deserialize((byte[])dict["data"]);

            if (HasProcessed(GetType(), player, playerProceedMedsPacket))
                return;

            if (!ItemFinder.TryFindItem(playerProceedMedsPacket.ItemId, out Item item))
            {
                Logger.LogError($"Unable to find Item {playerProceedMedsPacket.ItemId}");
                return;
            }
                if (item is Meds meds)
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

                                        if (playerProceedMedsPacket.AIMedicineType == "FirstAid")
                                        {
                                            FirstAid firstAid = botOwner.Medecine.FirstAid;
                                            firstAid.Using = false;
                                            firstAid.CheckParts();
                                            firstAid.Refresh();
                                            ReflectionHelpers.InvokeMethodForObject(firstAid, "FirstAidApplied");
                                        }
                                        else if (playerProceedMedsPacket.AIMedicineType == "SurgicalKit")
                                        {
                                            SurgicalKit surgicalKit = botOwner.Medecine.SurgicalKit;
                                            surgicalKit.Using = false;
                                            botOwner.AITaskManager.RegisterDelayedTask(botOwner, 1f, new Action(surgicalKit.FindDamagedPart));
                                            surgicalKit.Refresh();

                                            botOwner.Medecine.FirstAid.CheckParts();
                                        }
                                        else if (playerProceedMedsPacket.AIMedicineType == "Stimulators")
                                        {
                                            Stimulators stimulators = botOwner.Medecine.Stimulators;
                                            stimulators.Using = false;
                                            stimulators.Refresh();
                                        }
                                    });
                                }
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
    }

    public class PlayerProceedMedsPacket : PlayerProceedPacket
    {
        public EBodyPart BodyPart { get; set; }

        public int AnimationVariant { get; set; }

        public string AIMedicineType { get; set; }

        public PlayerProceedMedsPacket(string profileId, string itemId, string templateId, EBodyPart bodyPart, int animationVariant, bool scheduled, string method) : base(profileId, itemId, templateId, scheduled, method)
        {
            BodyPart = bodyPart;
            AnimationVariant = animationVariant;

            AIMedicineType = "";
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(TemplateId);
            writer.Write(Scheduled);
            writer.Write(BodyPart.ToString());
            writer.Write(AnimationVariant);
            writer.Write(TimeSerializedBetter);
            writer.Write(AIMedicineType);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            ItemId = reader.ReadString();
            TemplateId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            BodyPart = (EBodyPart)Enum.Parse(typeof(EBodyPart), reader.ReadString());
            AnimationVariant = reader.ReadInt32();
            TimeSerializedBetter = reader.ReadString();
            AIMedicineType = reader.ReadString();

            return this;
        }
    }
}