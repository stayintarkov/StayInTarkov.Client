using StayInTarkov.Coop.Components.CoopGameComponents;
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
        public PlayerStatesPacket() : base(nameof(PlayerStatesPacket))
        {

        }

        public PlayerStatesPacket(in PlayerStatePacket[] statePackets) : base(nameof(PlayerStatesPacket))
        {
            PlayerStates = statePackets;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter binaryWriter = new BinaryWriter(ms);
            WriteHeader(binaryWriter);
            binaryWriter.Write(PlayerStates.Length);
            foreach (var state in PlayerStates)
                binaryWriter.WriteLengthPrefixedBytes(state.Serialize());
            binaryWriter.Write(TimeSerializedBetter);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            var length = reader.ReadInt32();
            PlayerStates = new PlayerStatePacket[length];
            for (var i = 0; i < length; i++)
                PlayerStates[i] = new PlayerStatePacket().Deserialize(reader.ReadLengthPrefixedBytes()) as PlayerStatePacket;
            TimeSerializedBetter = reader.ReadString();
            return this;
        }

        public override void Process()
        {
            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                coopGameComponent.UpdatePing(GetTimeSinceSent().Milliseconds);

            for (var i = 0; i < PlayerStates.Length; i++)
                PlayerStates[i].Process();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerStatesPacket)}:{nameof(Dispose)}");
                for (var i = 0; i < PlayerStates.Length; i++)
                {
                    PlayerStates[i].Dispose();
                    PlayerStates[i] = null;
                }
                PlayerStates = null;
            }
            base.Dispose(disposing);
        }
    }
}
