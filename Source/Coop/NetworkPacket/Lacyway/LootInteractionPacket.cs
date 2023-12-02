using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct LootInteractionPacket
    {
        public bool Interact {  get; set; }
        public string LootId { get; set; }
        public uint CallbackId { get; set; }
    }
}
