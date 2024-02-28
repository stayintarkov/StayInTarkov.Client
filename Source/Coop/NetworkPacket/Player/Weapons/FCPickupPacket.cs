using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public class FCPickupPacket : BasePlayerPacket
    {
        public bool Pickup { get; set; }

        public FCPickupPacket(string profileId, bool pickup) : base(profileId, "FCPickup")
        {
            Pickup = pickup;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Pickup);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Pickup = reader.ReadBoolean();
            return this;
        }


    }
}
