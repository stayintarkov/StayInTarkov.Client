using StayInTarkov.Networking.Packets;
using System.Collections.Generic;

namespace StayInTarkov.Coop.PacketQueues
{
    public class InventoryPacketQueue : Queue<InventoryPacket>
    {
        public InventoryPacketQueue(int capacity) : base(capacity)
        {

        }
    }
}
