using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public struct StationaryPacket : ISITPacket, INetSerializable
    {
        public EStationaryCommand Command { get; set; }
        public string Id { get; set; }
        public string ServerId { get; set; }
        public string TimeSerializedBetter { get; set; }
        public string Method { get; set; }

        public enum EStationaryCommand : byte
        {
            Occupy,
            Leave,
            Denied
        }

        public StationaryPacket Deserialize(NetDataReader reader)
        {
            StationaryPacket packet = new()
            {
                Command = (EStationaryCommand)reader.GetByte()
            };

            if (packet.Command == EStationaryCommand.Occupy)
                packet.Id = reader.GetString();

            return packet;
        }
        public static void Serialize(NetDataWriter writer, StationaryPacket packet)
        {
            writer.Put((byte)packet.Command);
            if (packet.Command == EStationaryCommand.Occupy && !string.IsNullOrEmpty(packet.Id))
                writer.Put(packet.Id);
        }

        void INetSerializable.Serialize(NetDataWriter writer)
        {
            var serializedSIT = Serialize();
            writer.Put(serializedSIT.Length);
            writer.Put(serializedSIT);
        }

        void INetSerializable.Deserialize(NetDataReader reader)
        {
            var length = reader.GetInt();
            byte[] bytes = new byte[length];
            reader.GetBytes(bytes, length);
            Deserialize(bytes);
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public ISITPacket Deserialize(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
