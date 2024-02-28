using Aki.Custom.Airdrops.Models;
using StayInTarkov.AkiSupport.Airdrops.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropLootPacket : BasePacket
    {
        public string AirdropLootResultModelJson { get; set; }
        public string AirdropConfigModelJson { get; set; }

        public AirdropLootPacket(AirdropLootResultModel airdropLootResultModel, AirdropConfigModel airdropConfigModel) : base("AirdropLootPacket")
        {
            AirdropLootResultModelJson = airdropLootResultModel.SITToJson();
            AirdropConfigModelJson = airdropConfigModel.SITToJson();
        }

        public AirdropLootPacket() : base("AirdropLootPacket")
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(AirdropLootResultModelJson);
            writer.Write(AirdropConfigModelJson);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            AirdropLootResultModelJson = reader.ReadString();
            AirdropConfigModelJson = reader.ReadString();
            return this;
        }
    }
}
