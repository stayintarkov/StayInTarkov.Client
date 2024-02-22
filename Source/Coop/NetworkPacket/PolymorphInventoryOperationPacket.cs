using SIT.Core.Coop.PacketHandlers;
using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PolymorphInventoryOperationPacket : ItemPlayerPacket
    {
        /// <summary>
        /// This is called via Reflection
        /// </summary>
        public PolymorphInventoryOperationPacket() : base("","","", "PolymorphInventoryOperationPacket")
        {

        }

        public PolymorphInventoryOperationPacket(string profileId, string itemId, string templateId) : base(profileId, itemId, templateId, "PolymorphInventoryOperationPacket")
        {

        }

        public override void Process()
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{GetType()}:{nameof(Process)}");
            if (Method == "PolymorphInventoryOperationPacket")
            {
                if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                {
                    PlayerInventoryPacketHandler.ProcessPolymorphOperation(coopGameComponent.Players[ProfileId], this);
                    return;
                }
            }
        }
    }
}
