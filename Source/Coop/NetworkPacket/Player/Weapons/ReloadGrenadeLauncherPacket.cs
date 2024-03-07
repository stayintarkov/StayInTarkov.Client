using EFT.InventoryLogic;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ReloadGrenadeLauncherPacket : BasePlayerPacket
    {
        public string[] AmmoIds { get; set; }

        public ReloadGrenadeLauncherPacket() : base("", nameof(ReloadGrenadeLauncherPacket)) { }

        public ReloadGrenadeLauncherPacket(string profileId, string[] ammoIds) : base(profileId, nameof(ReloadGrenadeLauncherPacket))
        {
            ProfileId = profileId;
            AmmoIds = ammoIds;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(AmmoIds.Length);
            foreach (var ammo in AmmoIds)
                writer.Write(ammo);

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
                firearmController.ReloadGrenadeLauncher(ammoPack, (x) => { });
            }
        }

    }
}
