using Newtonsoft.Json;

namespace SIT.Core.Coop.NetworkPacket
{
    public class BasePlayerPacket : BasePacket
    {
        //[JsonProperty(PropertyName = "accountId")]
        //public string AccountId { get { return ProfileId; } set { ProfileId = value; } }

        [JsonProperty(PropertyName = "profileId")]
        public string ProfileId { get; set; }

        public BasePlayerPacket()
        {
        }

        public BasePlayerPacket(string profileId, string method)
        {
            //AccountId = accountId;
            ProfileId = profileId;
            Method = method;
        }
    }
}
