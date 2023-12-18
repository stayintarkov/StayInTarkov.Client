using LiteNetLib.Utils;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Networking.Packets
{
    public struct InventoryPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool HasItemControllerExecutePacket { get; set; }
        public ItemControllerExecutePacket ItemControllerExecutePacket { get; set; }
        public bool HasItemMovementHandlerMovePacket { get; set; }
        public ItemMovementHandlerMovePacket ItemMovementHandlerMovePacket { get; set; }
        public InventoryPacket(string profileId) 
        {
            ProfileId = profileId;
            HasItemControllerExecutePacket = false;
            HasItemMovementHandlerMovePacket = false;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(HasItemControllerExecutePacket);
            if (HasItemControllerExecutePacket)
                ItemControllerExecutePacket.Serialize(writer, ItemControllerExecutePacket);
            writer.Put(HasItemMovementHandlerMovePacket);
            if (HasItemMovementHandlerMovePacket)
                ItemMovementHandlerMovePacket.Serialize(writer, ItemMovementHandlerMovePacket);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            HasItemControllerExecutePacket = reader.GetBool();
            if (HasItemControllerExecutePacket)
                ItemControllerExecutePacket = ItemControllerExecutePacket.Deserialize(reader);
            HasItemMovementHandlerMovePacket = reader.GetBool();
            if (HasItemMovementHandlerMovePacket)
                ItemMovementHandlerMovePacket.Deserialize(reader);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
