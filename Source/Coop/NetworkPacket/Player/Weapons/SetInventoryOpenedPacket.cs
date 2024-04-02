using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class SetInventoryOpenedPacket : BasePlayerPacket
    {
        public bool Opened {  get; set; }

        public SetInventoryOpenedPacket() : base("", nameof(SetInventoryOpenedPacket))
        {
        }

        public SetInventoryOpenedPacket(string profileId, bool opened) : base(profileId, nameof( SetInventoryOpenedPacket))
        {
            Opened = opened;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Opened);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Opened = reader.ReadBoolean();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var inventoryOpen = client.HandsController as EFT.Player.FirearmController;
            if (inventoryOpen != null)
            {
                inventoryOpen.SetInventoryOpened(Opened);
            }
        }
    }
}
