using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.Coop.Matchmaker;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropPacket : BasePacket
    {
        static AirdropPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(AirdropPacket));
        }

        private static ManualLogSource Logger;

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
            if (!SITMatchmaking.IsClient)
                return;

            if (!Singleton<SITAirdropsManager>.Instantiated)
            {
                Logger.LogError($"{nameof(SITAirdropsManager)} has not been instantiated!");
                return;
            }

            Singleton<SITAirdropsManager>.Instance.AirdropParameters = this.AirdropParametersModelJson.SITParseJson<AirdropParametersModel>();

#if DEBUG
            Logger.LogDebug($"{nameof(SITAirdropsManager)}{nameof(Singleton<SITAirdropsManager>.Instance.AirdropParameters)}");
            Logger.LogDebug($"{Singleton<SITAirdropsManager>.Instance.AirdropParameters.SITToJson()}");
#endif

        }
    }
}
