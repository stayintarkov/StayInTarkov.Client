using System;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public class GrenadeThrowPacket : BasePlayerPacket
    {
        public float rX { get; set; }

        public float rY { get; set; }

        public GrenadeThrowPacket() { }

        public GrenadeThrowPacket(string profileId, UnityEngine.Vector2 rotation, string method) : base(profileId, method)
        {
            rX = rotation.x;
            rY = rotation.y;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(rX);
            writer.Write(rY);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            rX = reader.ReadSingle();
            rY = reader.ReadSingle();

            return this;
        }
    }
}