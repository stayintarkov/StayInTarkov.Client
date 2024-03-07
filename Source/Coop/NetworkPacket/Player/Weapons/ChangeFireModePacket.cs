using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EFT.InventoryLogic.Weapon;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ChangeFireModePacket : BasePlayerPacket
    {
        public EFireMode FireMode { get; set; }

        public ChangeFireModePacket() : base("", nameof(ChangeFireModePacket)) { }

        public ChangeFireModePacket(string profileId, EFireMode mode) : base(profileId, nameof(ChangeFireModePacket)) { FireMode = mode; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write((byte)FireMode);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            FireMode = (EFireMode)reader.ReadByte();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.ChangeFireMode(FireMode);
            }
        }
    }
}
