using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Coop.Matchmaker;
using System.IO;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Airdrop
{
    public sealed class AirdropBoxPositionSyncPacket : BasePacket
    {
        static AirdropBoxPositionSyncPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(AirdropPacket));
        }

        public AirdropBoxPositionSyncPacket() : base(nameof(AirdropBoxPositionSyncPacket))
        {
        }

        public Vector3 Position { get; set; }
        public static ManualLogSource Logger { get; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);
            Vector3Utils.Serialize(writer, this.Position);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            Position = Vector3Utils.Deserialize(reader);
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


            //Logger.LogDebug($"{nameof(Process)}");

            Singleton<SITAirdropsManager>.Instance.AirdropBox.ClientSyncPosition = Position;
        }

    }
}
