using EFT;
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
    public class PlayerProceedFoodDrinkPacket : PlayerProceedPacket
    {
        public float Amount { get; set; }

        public int AnimationVariant { get; set; }

        public bool UsedAll { get; set; }

        public PlayerProceedFoodDrinkPacket() { }

        public PlayerProceedFoodDrinkPacket(string profileId, string itemId, string templateId, float amount, int animationVariant, bool scheduled) : base(profileId, itemId, templateId, scheduled, nameof(PlayerProceedFoodDrinkPacket))
        {
            Amount = amount;
            AnimationVariant = animationVariant;
        }

        public override byte[] Serialize()
        {
            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(PlayerProceedFoodDrinkPacket)}:{nameof(Serialize)}"); 

            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(TemplateId);
            writer.Write(Scheduled);
            writer.Write((Single)Amount);
            writer.Write(AnimationVariant);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(PlayerProceedFoodDrinkPacket)}:{nameof(Deserialize)}");

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            TemplateId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            Amount = reader.ReadSingle();
            AnimationVariant = reader.ReadInt32();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        public override void Process()
        {
            if (Method != nameof(PlayerProceedFoodDrinkPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
            {
                if (ItemFinder.TryFindItem(this.ItemId, out Item item) && item is FoodClass foodDrink)
                {
                    client.ReceivedPackets.Enqueue(this);
                }
            }
            //StayInTarkovPlugin.Instance.StartCoroutine(ProceedCoroutine());
        }

        //private IEnumerator ProceedCoroutine()
        //{
        //    bool done = false;
        //    while (!done)
        //    {
        //        if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
        //            break;

        //        if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
        //        {
        //            if (ItemFinder.TryFindItem(this.ItemId, out Item item) && item is FoodClass foodDrink)
        //            {
        //                //client.ReceivedFoodDrinkPacket = this;
        //                //client.Proceed(foodDrink, this.Amount, null, this.AnimationVariant, this.Scheduled);
        //                done = true;
        //                client.ReceivedPackets.Enqueue(this);
        //            }
        //            else
        //                break;
        //        }
        //        else
        //            break;

        //        yield return new WaitForSeconds(10);
        //    }

        //}

        public override bool Equals(object obj)
        {
            if(obj is PlayerProceedFoodDrinkPacket foodDrinkPacket)
            {
                if(TimeSerializedBetter == foodDrinkPacket.TimeSerializedBetter) 
                    return true;  
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
