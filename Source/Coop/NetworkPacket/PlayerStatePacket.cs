using Newtonsoft.Json;

namespace SIT.Core.Coop.NetworkPacket
{
    public class PlayerStatePacket : BasePlayerPacket
    {
        [JsonProperty(PropertyName = "dX")]
        public float dX { get; set; }

        [JsonProperty(PropertyName = "dY")]
        public float dY { get; set; }

        [JsonProperty(PropertyName = "pX")]
        public float pX { get; set; }

        [JsonProperty(PropertyName = "pY")]
        public float pY { get; set; }

        [JsonProperty(PropertyName = "pZ")]
        public float pZ { get; set; }

        [JsonProperty(PropertyName = "rX")]
        public float rX { get; set; }

        [JsonProperty(PropertyName = "rY")]
        public float rY { get; set; }

        [JsonProperty(PropertyName = "pose")]
        public float pose { get; set; }

        [JsonProperty(PropertyName = "spd")]
        public float spd { get; set; }

        [JsonProperty(PropertyName = "spr")]
        public bool spr { get; set; }

        [JsonProperty(PropertyName = "tp")]
        public bool tp { get; set; }

        [JsonProperty(PropertyName = "alive")]
        public bool alive { get; set; }

        [JsonProperty(PropertyName = "tilt")]
        public float Tilt { get; set; }

        [JsonProperty(PropertyName = "prn")]
        public bool prn { get; set; }

        [JsonProperty(PropertyName = "m")]
        public override string Method { get => "PlayerState"; }


    }
}
