using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.NetworkPacket
{
    public class PlayerMovePacket : BasePlayerPacket, IDisposable
    {
        [JsonProperty(PropertyName = "pX")]
        public float pX { get; set; }

        [JsonProperty(PropertyName = "pY")]
        public float pY { get; set; }

        [JsonProperty(PropertyName = "pZ")]
        public float pZ { get; set; }

        public float dX { get; set; }
        public float dY { get; set; }
        public float spd { get; set; }

        public PlayerMovePacket() : base(null, "Move")
        {

        }

        public PlayerMovePacket(string profileId) : base(profileId, "Move")
        {
        }

        public void Dispose()
        {
            ProfileId = null;
            Method = null;
        }
    }
}
