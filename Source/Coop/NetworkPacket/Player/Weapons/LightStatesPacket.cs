using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class LightStatesPacket : BasePlayerPacket
    {
        public LightsStates[] LightStates { get; set; }

        public LightStatesPacket() : base("", nameof(LightStatesPacket)) { }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(LightStates.Length);
            for (var i = 0; i < LightStates.Length; i++)
            {
                writer.Write(LightStates[i].Id);
                writer.Write(LightStates[i].IsActive);
                writer.Write(LightStates[i].LightMode);
            }

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            var length = reader.ReadInt32();
            LightStates = new LightsStates[length];
            for (var i = 0; i < length; i++)
            {
                LightStates[i] = new LightsStates();
                LightStates[i].Id = reader.ReadString();
                LightStates[i].IsActive = reader.ReadBoolean();
                LightStates[i].LightMode = reader.ReadInt32();
            }
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.SetLightsState(LightStates, true);
            }
        }

    }
}
