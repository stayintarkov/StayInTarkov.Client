using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerHealthPacket : BasePlayerPacket
    {
        public bool IsAlive { get; set; }   
        public float Energy { get; set; }
        public float Hydration { get; set; }
        public float Radiation { get; set; }
        public float Poison { get; set; }
        public PlayerBodyPartHealthPacket[] BodyParts { get; set; }

        public PlayerHealthPacket(string profileId) : base(profileId, "PlayerHealth") { }
    }
}
