using Aki.Custom.Airdrops;
using Comfort.Common;
using StayInTarkov.AkiSupport.Airdrops.Models;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropPacket : BasePacket
    {
        public string AirdropParametersModelJson { get; set; }

        public AirdropPacket(AirdropParametersModel airdropParametersModel) : base(nameof(AirdropPacket))
        {
            AirdropParametersModelJson = airdropParametersModel.SITToJson();
        }

        public AirdropPacket() : base(nameof(AirdropPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);
            writer.Write(AirdropParametersModelJson);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            AirdropParametersModelJson = reader.ReadString();
            return this;
        }

        public override void Process()
        {
            if (!Singleton<SITAirdropsManager>.Instantiated)
                return;

            Singleton<SITAirdropsManager>.Instance.AirdropParameters = this.AirdropParametersModelJson.SITParseJson<AirdropParametersModel>();
        }
    }
}
