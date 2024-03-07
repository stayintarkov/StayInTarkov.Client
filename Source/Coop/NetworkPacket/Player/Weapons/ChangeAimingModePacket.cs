using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ChangeAimingModePacket : BasePlayerPacket
    {
        public int AimingIndex { get; set; }

        public ChangeAimingModePacket() : base("", nameof(ChangeAimingModePacket)) { }

        public ChangeAimingModePacket(string profileId, int aimingIndex) : base(profileId, nameof(ChangeAimingModePacket)) { AimingIndex = aimingIndex; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(AimingIndex);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            AimingIndex = reader.ReadInt32();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.Item.AimIndex.Value = AimingIndex;
                firearmController.ChangeAimingMode();
            }
        }
    }
}