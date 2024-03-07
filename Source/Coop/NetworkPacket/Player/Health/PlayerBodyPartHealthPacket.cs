using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public class PlayerBodyPartHealthPacket : BasePacket
    {
        public EBodyPart BodyPart { get; set; }
        public float Current { get; set; }
        public float Maximum { get; set; }

        public PlayerBodyPartHealthPacket() : base("PlayerBodyPartHealth")
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write((byte)BodyPart);
            writer.Write(Current);
            writer.Write(Maximum);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            BodyPart = (EBodyPart)reader.ReadByte(); // body part
            Current = reader.ReadSingle();
            Maximum = reader.ReadSingle();
            return this;
        }

        public override void Process()
        {
            // Doesn't do anything
        }
    }
}
