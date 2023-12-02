using EFT.NetworkPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct PrevFrame
    {
        public EHandsTypePacket HandsTypePacket {  get; set; }
        public MovementInfoPacket MovementInfoPacket { get; set; }
        public HandsChangePacket HandsChangePacket { get; set; }

    }
}
