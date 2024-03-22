using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class RequestSpawnPlayersPacket : BasePacket
    {
        public string[] ExistingProfileIds { get; set; }

        public RequestSpawnPlayersPacket() : base(nameof(RequestSpawnPlayersPacket))
        {

        }

        public RequestSpawnPlayersPacket(in string[] existingProfileIds) : this()
        {
            ExistingProfileIds = existingProfileIds;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ExistingProfileIds.Length);
            foreach (var profileId in ExistingProfileIds)
            {
                writer.Write(profileId);
            }

            return base.Serialize();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));

            var length = reader.ReadInt();
            ExistingProfileIds = new string[length];
            for(var i = 0; i < length; i++)
            {
                ExistingProfileIds[i] = reader.ReadString();
            }

            return this;
        }

        public override void Process()
        {
            base.Process();
        }

    }
}
