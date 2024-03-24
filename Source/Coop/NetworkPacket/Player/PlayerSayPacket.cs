using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public sealed class PlayerSayPacket : BasePlayerPacket
    {
        public PlayerSayPacket() : base("", nameof(PlayerSayPacket))
        {

        }

        public EPhraseTrigger Trigger { get; set; }
        public int Index { get; set; }
        public bool Aggressive { get; internal set; }
        public ETagStatus Mask { get; internal set; }
        public float Delay { get; internal set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write((byte)Trigger);
            writer.Write(Index);
            writer.Write(Aggressive);
            writer.Write((uint)Mask);
            writer.Write(Delay);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Trigger = (EPhraseTrigger)reader.ReadByte();
            Index = reader.ReadInt32();
            Aggressive = reader.ReadBoolean();
            Mask = (ETagStatus)reader.ReadUInt32();
            Delay = reader.ReadSingle();
            return this;
        }

        public override void Process()
        {
            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            if (!coopGameComponent.Players.ContainsKey(ProfileId))
                return;

            var player = coopGameComponent.Players[ProfileId];
            player.ReceiveSay(Trigger, Index, Mask, Aggressive);
        }
    }
}
