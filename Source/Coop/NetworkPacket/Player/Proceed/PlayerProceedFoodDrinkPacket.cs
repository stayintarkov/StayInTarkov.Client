using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public class PlayerProceedFoodDrinkPacket : PlayerProceedPacket
    {
        public float Amount { get; set; }

        public int AnimationVariant { get; set; }

        public PlayerProceedFoodDrinkPacket() { }

        public PlayerProceedFoodDrinkPacket(string profileId, string itemId, string templateId, float amount, int animationVariant, bool scheduled) : base(profileId, itemId, templateId, scheduled, "PlayerProceedFoodDrinkPacket")
        {
            Amount = amount;
            AnimationVariant = animationVariant;
        }

        public override byte[] Serialize()
        {
            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(PlayerProceedFoodDrinkPacket)}:{nameof(Serialize)}"); 

            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
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
            ReadHeader(reader);
            ProfileId = reader.ReadString();
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

            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                if (ItemFinder.TryFindItem(this.ItemId, out Item item) && item is FoodDrink foodDrink)
                {
                    // If the player exists, process
                    if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
                    {
                        client.Proceed(foodDrink, this.Amount, null, this.AnimationVariant, this.Scheduled);
                    }
                    else
                    {
                        // If the player doesn't exist, hold the packet until they do exist
                        Task.Run(async () =>
                        {

                            while (true)
                            {
                                await Task.Delay(10 * 1000);

                                if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
                                {
                                    client.Proceed(foodDrink, this.Amount, null, this.AnimationVariant, this.Scheduled);
                                    break;
                                }
                            }

                        });
                    }
                }
                return;
            }
        }
    }
}
