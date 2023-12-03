

using System;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    public struct HelmetLightPacket
    {
        public bool IsSilent {  get; set; }
        public LightsStates[] LightsStates { get; set; }
    }

}