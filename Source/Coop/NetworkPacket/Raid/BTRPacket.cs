using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Multiplayer.BTR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class BTRPacket : BasePacket
    {
        static BTRPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BTRPacket));

        }
        public BTRPacket() : base(nameof(BTRPacket))
        {
        }
        public static ManualLogSource Logger { get; }

        public string BotProfileId { get; set; }

        public Vector3? ShotDirection { get; set; }
        public Vector3? ShotPosition { get; set; }

        public bool HasShot { get { return ShotDirection.HasValue && ShotPosition.HasValue; } }

        public BTRDataPacket DataPacket { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);

            writer.Write(string.IsNullOrEmpty(BotProfileId));
            if (!string.IsNullOrEmpty(BotProfileId))
                writer.Write(BotProfileId);

            if (HasShot)
            {
                Vector3Utils.Serialize(writer, ShotDirection.Value);
                Vector3Utils.Serialize(writer, ShotPosition.Value);
            }


            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            return this;
        }

        public override void Process()
        {
            if (!Singleton<BTRManager>.Instantiated)
            {
                Logger.LogError($"{nameof(BTRManager)} has not been instantiated!");
                return;
            }


        }
    }
}
