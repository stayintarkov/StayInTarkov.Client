using StayInTarkov.Coop.NetworkPacket.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class SpawnPlayersPacket : BasePacket
    {
        public PlayerInformationPacket[] InformationPackets { get; set; }

        public SpawnPlayersPacket() : base(nameof(SpawnPlayersPacket))
        {

        }

        public SpawnPlayersPacket(in PlayerInformationPacket[] informationPackets) : this()
        {
            InformationPackets = informationPackets;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(InformationPackets.Length);
            foreach (var packet in InformationPackets)
            {
                writer.WriteLengthPrefixedBytes(packet.Serialize());
            }

            return base.Serialize();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));

            var length = reader.ReadInt();
            InformationPackets = new PlayerInformationPacket[length];
            for (var i = 0; i < length; i++)
            {
                InformationPackets[i] = new PlayerInformationPacket();
                InformationPackets[i].Deserialize(reader.ReadLengthPrefixedBytes());
            }

            return this;
        }

        public override void Process()
        {
            base.Process();
        }
    }
}
