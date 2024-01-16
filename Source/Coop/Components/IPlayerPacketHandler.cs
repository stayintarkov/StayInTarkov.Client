using System.Collections.Generic;

namespace StayInTarkov.Coop.Components
{
    internal interface IPlayerPacketHandler
    {
        public void ProcessPacket(Dictionary<string, object> packet);
        public void ProcessPacket(byte[] packet);
    }
}
