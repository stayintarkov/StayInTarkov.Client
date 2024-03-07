using System.IO;
using System;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public class PlayerProceedPacket : ItemPlayerPacket
    {
        public bool Scheduled { get; set; }

        public PlayerProceedPacket() : base("", "", "", "")
        {

        }

        public PlayerProceedPacket(string profileId, string itemId, string templateId, bool scheduled, string method)
            : base(profileId, itemId, templateId, method)
        {
            Scheduled = scheduled;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(TemplateId);
            writer.Write(Scheduled);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            ItemId = reader.ReadString();
            TemplateId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

      
    }
}