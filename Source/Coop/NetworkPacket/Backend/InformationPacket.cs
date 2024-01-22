using LiteNetLib.Utils;

namespace StayInTarkov.Coop.NetworkPacket.Backend
{
    public struct InformationPacket(bool isRequest) : INetSerializable
    {
        public bool IsRequest { get; set; } = isRequest;
        public int NumberOfPlayers { get; set; } = 0;


        public void Deserialize(NetDataReader reader)
        {
            IsRequest = reader.GetBool();
            NumberOfPlayers = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsRequest);
            writer.Put(NumberOfPlayers);
        }
    }
}
