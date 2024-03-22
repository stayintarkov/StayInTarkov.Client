using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public class PlayerProceedMedsPacket : PlayerProceedPacket
    {
        public EBodyPart BodyPart { get; set; }

        public int AnimationVariant { get; set; }

        public string AIMedicineType { get; set; }

        public float Amount { get; set; }

        public bool UsedAll { get; set; }

        public PlayerProceedMedsPacket() : this("","","", EBodyPart.Head, 1, false, 1f) 
        { 
        
        
        }

        public PlayerProceedMedsPacket(string profileId, string itemId, string templateId, EBodyPart bodyPart, int animationVariant, bool scheduled, float amountUsed) : base(profileId, itemId, templateId, scheduled, nameof(PlayerProceedMedsPacket))
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
            writer.Write(Amount);
            writer.Write(UsedAll);

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
            Amount = reader.ReadSingle();
            UsedAll = reader.ReadBoolean();

            return this;
        }

        public override void Process()
        {
            if (Method != nameof(PlayerProceedMedsPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            StayInTarkovPlugin.Instance.StartCoroutine(ProceedCoroutine());
        }

        private IEnumerator ProceedCoroutine()
        {
            // will continue to run until a break is hit
            while (true)
            {
                if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    break;

                if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
                {
                    if (ItemFinder.TryFindItem(this.ItemId, out Item item) && item is MedsClass meds)
                    {
                        client.ReceivedPackets.Enqueue(this);
                        break;
                    }
                }
                else
                    break;

                yield return new WaitForSeconds(10);
            }

        }
    }
}
