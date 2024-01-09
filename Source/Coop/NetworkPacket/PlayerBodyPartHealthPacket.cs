using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
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
            writer.Write(BodyPart.ToString());
            writer.Write(Current);
            writer.Write(Maximum);
            var bytes = ms.ToArray();
            return bytes;
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            BodyPart = (EBodyPart)Enum.Parse(typeof(EBodyPart), reader.ReadString()); // body part
            Current = reader.ReadSingle();
            Maximum = reader.ReadSingle();
            return this;
        }
    }
}
