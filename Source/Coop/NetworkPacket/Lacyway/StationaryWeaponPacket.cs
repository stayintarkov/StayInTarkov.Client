using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct StationaryWeaponPacket
    {
        public bool HasInteraction { get; set; }
        public string Id { get; set; }
        public EStationaryCommand StationaryCommand { get; set; }
        public bool FinalizeObserverCrutch { get; set; }
        public bool IsStationaryFinal {  get; set; }
    }

    public enum EStationaryCommand : byte
    {
        Occupy,
        Leave,
        Denied
    }
}
