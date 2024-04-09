using EFT;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public sealed class PlayerProceedEmptyHandsPacket : BasePlayerPacket
    {
        public bool WithNetwork { get; set; }

        public bool Scheduled { get; set; }

        public PlayerProceedEmptyHandsPacket() : base("", nameof(PlayerProceedEmptyHandsPacket))
        { }

        public PlayerProceedEmptyHandsPacket(string profileId, bool withNetwork, bool scheduled) : base(profileId, nameof(PlayerProceedEmptyHandsPacket))
        {
            WithNetwork = withNetwork;
            Scheduled = scheduled;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Scheduled);
            writer.Write(WithNetwork);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Scheduled = reader.ReadBoolean();
            WithNetwork = reader.ReadBoolean();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            client.Proceed(this.WithNetwork, null, this.Scheduled);
        }
    }
}
