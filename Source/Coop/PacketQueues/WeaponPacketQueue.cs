using StayInTarkov.Networking.Packets;
using System.Collections.Generic;

namespace StayInTarkov.Coop.PacketQueues
{
    public class WeaponPacketQueue : Queue<WeaponPacket>
    {
        public WeaponPacketQueue(int capacity) : base(capacity)
        {

        }
    }
}
