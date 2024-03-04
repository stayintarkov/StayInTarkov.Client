using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ScopeStatesPacket : BasePlayerPacket
    {
        public ScopeStates[] ScopeStates { get; set; }

        public ScopeStatesPacket() : base("", nameof(ScopeStatesPacket)) { }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ScopeStates.Length);
            for (var i = 0; i < ScopeStates.Length; i++)
            {
                writer.Write(ScopeStates[i].Id);
                writer.Write(ScopeStates[i].ScopeMode);
                writer.Write(ScopeStates[i].ScopeIndexInsideSight);
                writer.Write(ScopeStates[i].ScopeCalibrationIndex);
            }

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            var length = reader.ReadInt32();
            ScopeStates = new ScopeStates[length];
            for (var i = 0; i < length; i++)
            {
                ScopeStates[i] = new ScopeStates();
                ScopeStates[i].Id = reader.ReadString();
                ScopeStates[i].ScopeMode = reader.ReadInt32();
                ScopeStates[i].ScopeIndexInsideSight = reader.ReadInt32();
                ScopeStates[i].ScopeCalibrationIndex = reader.ReadInt32();
            }
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.SetScopeMode(ScopeStates);
            }
        }
    }
}
