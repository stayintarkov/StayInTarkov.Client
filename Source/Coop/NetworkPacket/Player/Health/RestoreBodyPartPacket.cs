using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public class RestoreBodyPartPacket : BasePlayerPacket
    {
        public string BodyPart { get; set; }
        public float HealthPenalty { get; set; }

        public RestoreBodyPartPacket() : base()
        {
            Method = "RestoreBodyPart";
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(BodyPart);
            writer.Write(HealthPenalty);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            BodyPart = reader.ReadString();
            HealthPenalty = reader.ReadSingle();
            return this;
        }
    }
}
