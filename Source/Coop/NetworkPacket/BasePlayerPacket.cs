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
            ReadHeaderAndProfileId(reader);
            return this;
        }

        protected void ReadHeaderAndProfileId(BinaryReader reader)
        {
            ReadHeader(reader);
            ProfileId = reader.ReadString();
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            return ms.ToArray();
        }

        protected void WriteHeaderAndProfileId(BinaryWriter writer)
        {
            WriteHeader(writer);
            writer.Write(ProfileId);
        }



        // -------------------------------------------------------------------------------------
        // Do not override here. This will break any AutoDeserialization on anything inheriting.
        // If you want to Serialize differently. Do so in the inherited class

        //public override ISITPacket Deserialize(byte[] bytes)
        //{
        //    using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
        //    ReadHeader(reader);

        //    // This is not a BasePlayerPacket?
        //    if (reader.BaseStream.Position >= reader.BaseStream.Length)
        //        return this;

        //    ProfileId = reader.ReadString();
        //    TimeSerializedBetter = reader.ReadString();
        //    return this;
        //}

        //public override byte[] Serialize()
        //{
        //    var ms = new MemoryStream();
        //    using BinaryWriter writer = new BinaryWriter(ms);
        //    WriteHeader(writer);
        //    writer.Write(ProfileId);
        //    writer.Write(TimeSerializedBetter);
        //    return ms.ToArray();
        //}
    }
}