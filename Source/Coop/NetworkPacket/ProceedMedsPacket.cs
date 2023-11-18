using Newtonsoft.Json;

namespace SIT.Core.Coop.NetworkPacket
{
    public class ProceedMedsPacket : ItemPlayerPacket
    {
        [JsonProperty(PropertyName = "bodyPart")]
        public string BodyPart { get; set; }

        [JsonProperty(PropertyName = "variant")]
        public int Variant { get; set; }

        public ProceedMedsPacket(
            string profileId, string itemId, string templateId, string bodyPart, int variant)
            : base(profileId, itemId, templateId, "ProceedMeds")
        {
            BodyPart = bodyPart;
            Variant = variant;
        }
    }
}
