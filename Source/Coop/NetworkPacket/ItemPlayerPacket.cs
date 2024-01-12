using System.IO;
using UnityStandardAssets.Water;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class ItemPlayerPacket : BasePlayerPacket
    {
        public string ItemId { get; set; }

        public string TemplateId { get; set; }

        public ItemPlayerPacket(string profileId, string itemId, string templateId, string method)
            : base(new string(profileId.ToCharArray()), method)
        {
            ItemId = itemId;
            TemplateId = templateId;
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();

            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return this;

            ItemId = reader.ReadString();
            TemplateId = reader.ReadString();

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(TemplateId);
            return ms.ToArray();
        }
    }
}