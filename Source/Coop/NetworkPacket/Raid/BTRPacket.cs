using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Multiplayer.BTR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public bool HasShot { get; set; }

        public Vector3 ShotDirection { get; set; }
        public Vector3 ShotPosition { get; set; }

        public BTRDataPacket DataPacket { get; set; }

        public override byte[] Serialize()
        {
            return base.Serialize();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
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
