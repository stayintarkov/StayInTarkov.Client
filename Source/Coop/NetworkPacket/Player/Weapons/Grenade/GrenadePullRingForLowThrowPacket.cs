using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons.Grenade
{
    public sealed class GrenadePullRingForLowThrowPacket : BasePlayerPacket
    {
        public GrenadePullRingForLowThrowPacket() : base("", nameof(GrenadePullRingForLowThrowPacket)) { }

        public GrenadePullRingForLowThrowPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(GrenadePullRingForLowThrowPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                if (client.HandsController is EFT.Player.GrenadeController gc)
                {
                    gc.PullRingForLowThrow();
                }
            }
        }
    }
}
