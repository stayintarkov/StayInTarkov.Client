using StayInTarkov.AkiSupport.Airdrops.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropPacket : BasePacket
    {
        public string AirdropParametersModelJson { get; set; }

        public AirdropPacket(AirdropParametersModel airdropParametersModel) : base("AirdropPacket")
        {
            AirdropParametersModelJson = airdropParametersModel.SITToJson();
        }

        public AirdropPacket() : base("AirdropPacket")
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(AirdropParametersModelJson);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            AirdropParametersModelJson = reader.ReadString();
            return this;
        }
    }
}
