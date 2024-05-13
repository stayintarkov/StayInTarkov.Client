using Aki.Custom.Airdrops;
using Aki.Custom.Airdrops.Models;
using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Coop.Matchmaker;
using System.Collections;
using System.IO;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropLootPacket : BasePacket
    {

        static AirdropLootPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(AirdropLootPacket));
        }

        private static ManualLogSource Logger { get; set; }

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
            if (!SITMatchmaking.IsClient)
                return;

            if (!Singleton<SITAirdropsManager>.Instantiated)
            {
                Logger.LogDebug($"{nameof(SITAirdropsManager)} has not been instantiated! Waiting...");
                StayInTarkovPlugin.Instance.StartCoroutine(AirdropLootPacketWaitAndProcess());
                return;
            }


            if (!this.AirdropLootResultModelJson.TrySITParseJson<AirdropLootResultModel>(out var airdropLootResultModel))
            {
                Logger.LogError($"{nameof(AirdropLootResultModel)} failed to deserialize!");
                return;
            }

            if (!this.AirdropConfigModelJson.TrySITParseJson<AirdropConfigModel>(out var airdropConfigModel))
            {
                Logger.LogError($"{nameof(AirdropConfigModel)} failed to deserialize!");
                return;
            }


            Singleton<SITAirdropsManager>.Instance.ReceiveBuildLootContainer(
                airdropLootResultModel,
                airdropConfigModel
            );


        }

        private IEnumerator AirdropLootPacketWaitAndProcess()
        {
            var waitForSec = new WaitForSeconds(5);
            while (!Singleton<SITAirdropsManager>.Instantiated)
                yield return waitForSec;

            Process();
        }
    }
}
