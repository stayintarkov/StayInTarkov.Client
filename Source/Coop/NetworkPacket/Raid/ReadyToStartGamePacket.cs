using Comfort.Common;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class ReadyToStartGamePacket : BasePlayerPacket
    {
        public ReadyToStartGamePacket()
        {
        }

        public ReadyToStartGamePacket(string profileId) : base(profileId, nameof(ReadyToStartGamePacket))
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
            // ----------------------------------------------------------------------------------------------
            // Receive the Packet
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(ReadyToStartGamePacket)}:{nameof(Process)}");
            EFT.UI.ConsoleScreen.Log($"{nameof(ReadyToStartGamePacket)}:{nameof(Process)}");
            // ----------------------------------------------------------------------------------------------

            Singleton<ISITGame>.Instance.ReadyPlayers++;
        }
    }
}
