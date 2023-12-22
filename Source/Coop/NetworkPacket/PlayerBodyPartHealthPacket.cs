using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerBodyPartHealthPacket : BasePlayerPacket
    {
        public EBodyPart BodyPart { get; set; }
        public float Current { get; set; }
        public float Maximum { get; set; }

        public PlayerBodyPartHealthPacket() : base("", "PlayerBodyPartHealth") { }
    }
}
