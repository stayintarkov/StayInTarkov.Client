using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct FireModePacket
    {
        public bool ChangeFireMode;
        public Weapon.EFireMode FireMode;
    }
}
