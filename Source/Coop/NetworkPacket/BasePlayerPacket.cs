using Newtonsoft.Json;
using System.IO;

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

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            return ms.ToArray();
        }
    }
}