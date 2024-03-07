using EFT.InventoryLogic;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ReloadBarrelsPacket : BasePlayerPacket
    {
        public string[] AmmoIds { get; set; }

        public GridItemAddress GridItemAddress { get; set; }


        public ReloadBarrelsPacket() : base("", nameof(ReloadBarrelsPacket)) { }

        public ReloadBarrelsPacket(string profileId, string[] ammoIds, GridItemAddress gridItemAddress) : base(profileId, nameof(ReloadBarrelsPacket))
        {
            ProfileId = profileId;
            AmmoIds = ammoIds;
            GridItemAddress = gridItemAddress;  
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(AmmoIds.Length);
            foreach (var ammo in AmmoIds)
                writer.Write(ammo);

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

            var length = reader.ReadInt32();
            AmmoIds = new string[length];
            for (var i = 0; i < length; i++)
                AmmoIds[i] = reader.ReadString();

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
            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            List<BulletClass> ammoList = new();
            foreach (string ammoId in AmmoIds)
            {
                var findItem = client.FindItemById(ammoId, false, true);
                if (findItem.Failed)
                {
                    continue;
                }
                Item item = client.FindItemById(ammoId, false, true).Value;
                BulletClass bulletClass = findItem.Value as BulletClass;
                if (bulletClass == null)
                    continue;
                ammoList.Add(bulletClass);
            }

            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                AmmoPack ammoPack = new(ammoList);
                firearmController.ReloadBarrels(ammoPack, GridItemAddress, (x) => { });
            }
        }

    }
}
