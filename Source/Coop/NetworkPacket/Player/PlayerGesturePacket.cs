using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public class PlayerGesturePacket : BasePlayerPacket
    {
        public PlayerGesturePacket() : base("", nameof(PlayerGesturePacket))
        {

        }

        public EGesture Gesture { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write((byte)Gesture);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Gesture = (EGesture)reader.ReadByte();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            client.vmethod_3(Gesture);
        }
    }
}
