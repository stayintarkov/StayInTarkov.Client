using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public sealed class PlayerStatesPacket : BasePacket
    {
        public PlayerStatePacket[] PlayerStates { get; set; }
        public PlayerStatesPacket() : base("PlayerStatesPacket")
        {

        }

        public PlayerStatesPacket(in PlayerStatePacket[] statePackets) : base("PlayerStatesPacket")
        {
            PlayerStates = statePackets;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter binaryWriter = new BinaryWriter(ms);
            WriteHeader(binaryWriter);
            binaryWriter.Write(PlayerStates.Length);
            foreach (var state in PlayerStates)
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
            PlayerStates = new PlayerStatePacket[length];
            for (var i = 0; i < length; i++)
            {
                PlayerStates[i] = new PlayerStatePacket().Deserialize(reader.ReadLengthPrefixedBytes()) as PlayerStatePacket;
            }
            return this;
        }

        public override void Process()
        {
            foreach (var psp in PlayerStates)
            {
                psp.Process();
            }
        }
    }
}
