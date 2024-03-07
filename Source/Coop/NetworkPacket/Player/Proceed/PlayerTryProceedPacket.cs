using EFT.InventoryLogic;
using EFT;
using StayInTarkov.Coop.Player.Proceed;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StayInTarkov.Coop.Components.CoopGameComponents;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public sealed class PlayerTryProceedPacket : PlayerProceedPacket
    {
        public EFT.InventoryLogic.Item Item { get; set; }

        public PlayerTryProceedPacket() { }

        public PlayerTryProceedPacket(string profileId, EFT.InventoryLogic.Item item, bool scheduled)
           : base(profileId, item.Id, item.TemplateId, scheduled, nameof(PlayerTryProceedPacket))
        {
            Item = item;
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            TemplateId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(TemplateId);
            writer.Write(Scheduled);
            writer.Write(TimeSerializedBetter);
            
            return ms.ToArray();
        }

        public override void Process()
        {
            if (Method != nameof(PlayerTryProceedPacket))
                return;

            base.Process();

        }

        protected override void Process(CoopPlayerClient client)
        {

            if (client is CoopPlayer)
                return;

            // Prevent compass switch code running twice.
            if (client.IsYourPlayer && this.TemplateId == "5f4f9eb969cdc30ff33f09db") // EYE MK.2 professional hand-held compass
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (ItemFinder.TryFindItem(this.ItemId, out Item item))
            {
                // Make sure Tagilla and Cultists are using correct callback.
                if (client.IsAI && item.Attributes.Any(x => x.Name == "knifeDurab"))
                {
                    BotOwner botOwner = client.AIData.BotOwner;
                    if (botOwner != null)
                    {
                        botOwner.WeaponManager.Selector.ChangeToMelee();
                        return;
                    }
                }

                client.TryProceed(item, (x) => { }, this.Scheduled);
            }
            else
            {
                // Prevent softlock the player by switching to empty hands.
                if (client.IsYourPlayer)
                    client.Proceed(true, null, true);
            }

        }
    }
}
