using LiteNetLib.Utils;

namespace StayInTarkov.Networking.Packets
{
    public struct GameTimerPacket(bool isRequest, long tick = 0) : INetSerializable
    {
        public bool IsRequest { get; set; } = isRequest;
        public long Tick { get; set; } = tick;

        public void Deserialize(NetDataReader reader)
        {
            IsRequest = reader.GetBool();
            Tick = reader.GetLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsRequest);
            writer.Put(Tick);
        }
    }
}
