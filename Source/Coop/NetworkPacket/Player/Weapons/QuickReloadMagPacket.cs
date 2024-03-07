using EFT.InventoryLogic;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GClass648;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    internal class QuickReloadMagPacket : ItemPlayerPacket
    {
        public QuickReloadMagPacket() : base("","","", nameof(QuickReloadMagPacket))
        {

        }

        public QuickReloadMagPacket(string profileId, string itemId) : base(profileId, itemId, "", nameof(QuickReloadMagPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if(ItemFinder.TryFindItem(ItemId, out var item) && item is MagazineClass magazine) 
            {
                var firearmController = client.HandsController as EFT.Player.FirearmController;
                if (firearmController != null)
                {
                    firearmController.QuickReloadMag(magazine, (x) => { });
                }
            }

        }
    }

}
