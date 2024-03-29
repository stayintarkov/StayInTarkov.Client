using Comfort.Common;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class HostStartingGamePacket : BasePacket
    {
        public HostStartingGamePacket() : base(nameof(HostStartingGamePacket))
        {
        }

        public override byte[] Serialize()
        {
            return base.Serialize();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            return base.Deserialize(bytes);
        }

        public override void Process()
        {
            // ------------------------------------------------------------------------------------------
            // Receive the Packet
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(HostStartingGamePacket)}:{nameof(Process)}");
            EFT.UI.ConsoleScreen.Log($"{nameof(HostStartingGamePacket)}:{nameof(Process)}");
            // ------------------------------------------------------------------------------------------

            Singleton<ISITGame>.Instance.HostReady = true;
        }

    }
}
