using System.Collections.Generic;

namespace StayInTarkov.Coop.Components
{
    public interface IPlayerPacketHandlerComponent
    {
        public void ProcessPacket(Dictionary<string, object> packet);
    }
}
