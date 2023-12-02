using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct PlantItemPacket
    {
        public bool Successful {  get; set; }
        public string ItemId {  get; set; }
        public string ZoneId {  get; set; }
    }
}
