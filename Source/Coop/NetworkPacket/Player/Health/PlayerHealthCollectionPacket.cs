using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    internal class PlayerHealthCollectionPacket : BasePacket
    {
        public PlayerHealthPacket[] PlayerHealthCollection { get; set; }

        public PlayerHealthCollectionPacket() : base("PlayerHealthCollectionPacket")
        {

        }

        public PlayerHealthCollectionPacket(in PlayerHealthPacket[] healthPackets) : base("PlayerHealthCollectionPacket")
        {
            PlayerHealthCollection = healthPackets;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter binaryWriter = new BinaryWriter(ms);
            WriteHeader(binaryWriter);
            binaryWriter.Write(PlayerHealthCollection.Length);
            foreach (var state in PlayerHealthCollection)
            {
                binaryWriter.WriteLengthPrefixedBytes(state.Serialize());
            }
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            var length = reader.ReadInt32();
            PlayerHealthCollection = new PlayerHealthPacket[length];
            for (var i = 0; i < length; i++)
            {
                PlayerHealthCollection[i] = new PlayerHealthPacket().Deserialize(reader.ReadLengthPrefixedBytes()) as PlayerHealthPacket;
            }
            return this;
        }
    }
}
