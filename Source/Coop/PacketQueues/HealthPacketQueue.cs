using StayInTarkov.Networking.Packets;
using System.Collections.Generic;

namespace StayInTarkov.Coop
{
    public class HealthPacketQueue : Queue<WeaponPacket>
    {
        public HealthPacketQueue(int capacity) : base(capacity)
        {

        }
    }
}
