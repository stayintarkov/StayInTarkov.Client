using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons.Grenade
{
    public sealed class GrenadeLowThrowPacket : BasePlayerPacket
    {
        public float RotationX { get; set; }
        public float RotationY { get; set; }

        public GrenadeLowThrowPacket() : base("", nameof(GrenadeLowThrowPacket)) { }

        public GrenadeLowThrowPacket(string profileId, Vector2 rotation) : base(new string(profileId.ToCharArray()), nameof(GrenadeLowThrowPacket))
        {
            RotationX = rotation.x;
            RotationY = rotation.y;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(RotationX);
            writer.Write(RotationY);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            RotationX = reader.ReadSingle();
            RotationY = reader.ReadSingle();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                if (client.HandsController is EFT.Player.GrenadeController gc)
                {
                    client.Rotation = new UnityEngine.Vector2(RotationX, RotationY);
                    gc.LowThrow();
                }
            }
        }
    }
}
