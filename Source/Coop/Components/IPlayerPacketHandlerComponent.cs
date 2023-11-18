using System.Collections.Generic;

namespace SIT.Core.Coop.Components
{
    internal interface IPlayerPacketHandlerComponent
    {
        public void ProcessPacket(Dictionary<string, object> packet);
    }
}
