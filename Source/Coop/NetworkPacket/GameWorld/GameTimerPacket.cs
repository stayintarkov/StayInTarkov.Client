using LiteNetLib.Utils;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Coop.NetworkPacket.GameWorld
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
