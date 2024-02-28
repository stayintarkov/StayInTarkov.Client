using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public sealed class TriggerPressedPacket : BasePlayerPacket
    {
        public bool pr { get; set; }
        public float rX { get; set; }
        public float rY { get; set; }

        public TriggerPressedPacket() {  }

        public TriggerPressedPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(TriggerPressedPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(pr);
            writer.Write(rX);
            writer.Write(rY);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            pr = reader.ReadBoolean();
            rX = reader.ReadSingle();
            rY = reader.ReadSingle();

            return this;
        }

        public override void Process()
        {
            if (Method != nameof(TriggerPressedPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                // If the player exists, process
                if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayer client)
                {
                    if (client.HandsController is EFT.Player.FirearmController fc)
                    {
                        fc.CurrentOperation.SetTriggerPressed(pr);
                        Dispose();
                    }
                }
                else
                {
                    // If the player doesn't exist, hold the packet until they do exist
                    Task.Run(async () =>
                    {

                        while (true)
                        {
                            await Task.Delay(10 * 1000);

                            if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayer client)
                            {
                                if (client.HandsController is EFT.Player.FirearmController fc)
                                {
                                    fc.CurrentOperation.SetTriggerPressed(pr);
                                    Dispose();
                                }
                            }
                        }

                    });
                }
            }
        }
    }
}
