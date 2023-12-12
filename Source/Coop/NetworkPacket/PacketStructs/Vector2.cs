using LiteNetLib.Utils;

namespace StayInTarkov.Coop.NetworkPacket.PacketStructs
{
    internal struct SITVector2 : INetSerializable
    {
        public float X;
        public float Y;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(X);
            writer.Put(Y);
        }

        public void Deserialize(NetDataReader reader)
        {
            X = reader.GetFloat();
            Y = reader.GetFloat();
        }
    }
}
