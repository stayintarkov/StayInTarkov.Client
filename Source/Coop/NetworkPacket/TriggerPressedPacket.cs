using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public sealed class TriggerPressedPacket : BasePlayerPacket
    {
        public bool pr { get; set; }
        public float rX { get; set; }
        public float rY { get; set; }

        public TriggerPressedPacket(string profileId) : base(new string(profileId.ToCharArray()), "SetTriggerPressed")
        {
        }

    }
}
