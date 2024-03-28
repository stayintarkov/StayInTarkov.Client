using EFT;
using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public class KillPacket : BasePlayerPacket
    {
        public EDamageType DamageType { get; set; }

        public KillPacket()
        {

        }

        public KillPacket(string profileId, EDamageType damageType) : base(new string(profileId.ToCharArray()), nameof(KillPacket))
        {
            DamageType = damageType;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write((uint)DamageType);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            DamageType = (EDamageType)reader.ReadUInt32();
            return this;
        }

        public override void Process()
        {
            if (Method != nameof(KillPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                // If the player exists, process
                if (coopGameComponent.Players.ContainsKey(ProfileId))
                    coopGameComponent.Players[ProfileId].ActiveHealthController.Kill(DamageType);
                else
                {
                    // If the player doesn't exist, hold the packet until they do exist
                    Task.Run(async () =>
                    {

                        while (true)
                        {
                            await Task.Delay(10 * 1000);

                            if (coopGameComponent.Players.ContainsKey(ProfileId))
                            {
                                coopGameComponent.Players[ProfileId].ActiveHealthController.Kill(DamageType);
                                break;
                            }
                        }

                    });
                }
                return;
            }
        }
    }
}
