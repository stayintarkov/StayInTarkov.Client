using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct PacketItemInteraction
    {
        public bool HasInteraction { get; set; }
        public EInteractionType EInteractionType { get; set; }
        public EInteractionStage EInteractionStage { get; set; }
        public string Id { get; set; }
        public string ItemId { get; set; }
    }
}
