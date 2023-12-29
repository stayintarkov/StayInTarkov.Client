using Newtonsoft.Json;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class BasePlayerPacket : BasePacket
    {
        [JsonProperty(PropertyName = "profileId")]
        public string ProfileId { get; set; }

        public BasePlayerPacket() : base(null)
        {
        }

        public BasePlayerPacket(string profileId, string method) : base(method)
        {
            if(profileId != null)
                ProfileId = new string(profileId.ToCharArray());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ProfileId.Clear();
                ProfileId = null;
            }
            //StayInTarkovHelperConstants.Logger.LogDebug("PlayerMovePacket.Dispose");
        }
    }
}