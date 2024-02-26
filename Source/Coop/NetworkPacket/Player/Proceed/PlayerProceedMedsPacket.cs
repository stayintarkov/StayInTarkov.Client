using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public class PlayerProceedMedsPacket : PlayerProceedPacket
    {
        public EBodyPart BodyPart { get; set; }

        public int AnimationVariant { get; set; }

        public string AIMedicineType { get; set; }

        public PlayerProceedMedsPacket(string profileId, string itemId, string templateId, EBodyPart bodyPart, int animationVariant, bool scheduled, string method) : base(profileId, itemId, templateId, scheduled, method)
        {
            BodyPart = bodyPart;
            AnimationVariant = animationVariant;

            AIMedicineType = "";
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
            writer.Write(BodyPart.ToString());
            writer.Write(AnimationVariant);
            writer.Write(TimeSerializedBetter);
            writer.Write(AIMedicineType);

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
            BodyPart = (EBodyPart)Enum.Parse(typeof(EBodyPart), reader.ReadString());
            AnimationVariant = reader.ReadInt32();
            TimeSerializedBetter = reader.ReadString();
            AIMedicineType = reader.ReadString();

            return this;
        }
    }
}
