using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityStandardAssets.Water;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public class PlayerHealthPacket : BasePlayerPacket
    {
        public bool IsAlive { get; set; }
        public float Energy { get; set; }
        public float Hydration { get; set; }
        public float Radiation { get; set; }
        public float Poison { get; set; }

        public PlayerBodyPartHealthPacket[] BodyParts { get; } = new PlayerBodyPartHealthPacket[Enum.GetValues(typeof(EBodyPart)).Length];

        public PlayerHealthEffectPacket[] HealthEffectPackets { get; set; }

        public PlayerHealthPacket() : base("", nameof(PlayerHealthPacket))
        {
        }

        public PlayerHealthPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerHealthPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(IsAlive);
            writer.Write(Energy);
            writer.Write(Hydration);
            writer.Write(Radiation);
            writer.Write(Poison);
            foreach (var part in BodyParts)
            {
                writer.WriteLengthPrefixedBytes(part.Serialize());
            }

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            IsAlive = reader.ReadBoolean();
            Energy = reader.ReadSingle();
            Hydration = reader.ReadSingle();
            Radiation = reader.ReadSingle();
            Poison = reader.ReadSingle();
            for (var i = 0; i < BodyParts.Length; i++)
            {
                BodyParts[i] = new PlayerBodyPartHealthPacket();
                BodyParts[i] = (PlayerBodyPartHealthPacket)BodyParts[i].Deserialize(reader.ReadLengthPrefixedBytes());
            }

            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is PlayerHealthPacket other)
            {
                if (other == null) return false;

                if (
                    other.ProfileId == ProfileId &&
                    other.Energy == Energy &&
                    other.Hydration == Hydration &&
                    other.IsAlive == IsAlive &&
                    other.BodyParts != null &&
                    BodyParts != null &&
                    other.BodyParts.First(x => x.BodyPart == EBodyPart.Common).Current == BodyParts.First(x => x.BodyPart == EBodyPart.Common).Current
                    )
                {
                    //StayInTarkovHelperConstants.Logger.LogInfo("PlayerHealthPacket is the same");
                    return true;
                }
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
