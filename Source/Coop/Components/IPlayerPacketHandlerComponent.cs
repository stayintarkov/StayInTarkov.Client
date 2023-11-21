using System.Collections.Generic;

namespace StayInTarkov.Coop.Components
{
    internal interface IPlayerPacketHandlerComponent
    {
        public void ProcessPacket(Dictionary<string, object> packet);
    }
}
