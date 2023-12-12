using LiteNetLib.Utils;

namespace StayInTarkov.Coop.NetworkPacket.PacketStructs
{
    internal struct SITVector3 : INetSerializable
    {
        public float X;
        public float Y;
        public float Z;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(X);
            writer.Put(Y);
            writer.Put(Z);
        }

        public void Deserialize(NetDataReader reader)
        {
            X = reader.GetFloat();
            Y = reader.GetFloat();
            Z = reader.GetFloat();
        }
    }
}
