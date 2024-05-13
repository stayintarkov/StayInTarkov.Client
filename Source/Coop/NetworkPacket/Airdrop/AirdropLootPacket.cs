using Aki.Custom.Airdrops;
using Aki.Custom.Airdrops.Models;
using Comfort.Common;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropLootPacket : BasePacket
    {
        public string AirdropLootResultModelJson { get; set; }
        public string AirdropConfigModelJson { get; set; }

        public AirdropLootPacket(AirdropLootResultModel airdropLootResultModel, AirdropConfigModel airdropConfigModel) : base(nameof(AirdropLootPacket))
        {
            AirdropLootResultModelJson = airdropLootResultModel.SITToJson();
            AirdropConfigModelJson = airdropConfigModel.SITToJson();
        }

        public AirdropLootPacket() : base(nameof(AirdropLootPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);
            writer.Write(AirdropLootResultModelJson);
            writer.Write(AirdropConfigModelJson);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            AirdropLootResultModelJson = reader.ReadString();
            AirdropConfigModelJson = reader.ReadString();
            return this;
        }

        public override void Process()
        {
            if (!Singleton<SITAirdropsManager>.Instantiated)
                return;


            Singleton<SITAirdropsManager>.Instance.ReceiveBuildLootContainer(
                this.AirdropLootResultModelJson.SITParseJson<AirdropLootResultModel>(),
                this.AirdropConfigModelJson.SITParseJson<AirdropConfigModel>()
            );
        }
    }
}
