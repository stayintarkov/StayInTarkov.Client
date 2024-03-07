using Comfort.Common;
using EFT;
using EFT.Hideout;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityStandardAssets.Water;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ReloadMagPacket : ItemPlayerPacket
    {
        public GridItemAddress GridItemAddress { get; set; }

        public ReloadMagPacket() : base("", "", "", nameof(ReloadMagPacket))
        {

        }

        public ReloadMagPacket(string profileId, string itemId, GridItemAddress gridItemAddress) : base(profileId, itemId, "", nameof(ReloadMagPacket))
        {
            GridItemAddress = gridItemAddress;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(TimeSerializedBetter);

            // Get Descriptor
            ItemAddressHelpers.ConvertItemAddressToDescriptor(GridItemAddress, out var descriptor);
            // Write bit check
            writer.Write(descriptor != null);
            // Write descriptor if its not null
            if (descriptor != null)
                SITSerialization.AddressUtils.SerializeGridItemAddressDescriptor(writer, (GridItemAddressDescriptor)descriptor);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            TimeSerializedBetter = reader.ReadString();

            // Check Descriptor Exists
            var descriptorExists = reader.ReadBoolean();
            if (!descriptorExists)
                return this;

            var gridItemAddressDescriptor = SITSerialization.AddressUtils.DeserializeGridItemAddressDescriptor(reader);

            if (!ItemFinder.TryFindItemController(ProfileId, out var itemController))
            {
                StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Deserialize)}:Unable to find ItemController for {ProfileId}");
                return this;
            }

            GridItemAddress = itemController.ToGridItemAddress(gridItemAddressDescriptor);

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (ItemFinder.TryFindItem(ItemId, out var item) && item is MagazineClass magazine)
            {
                var firearmController = client.HandsController as EFT.Player.FirearmController;
                if (firearmController != null)
                {
                    firearmController.ReloadMag(magazine, GridItemAddress, (x) => { });
                }
            }

        }
    }
}
