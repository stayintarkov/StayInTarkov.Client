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
using static BackendConfigSettingsClass;
using static EFT.HealthSystem.ActiveHealthController;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    internal class PlayerHealthEffectPacket : BasePlayerPacket
    {
        public bool Add { get; set; }
        public AbstractEffect Effect { get; set; }    

        public string EffectType { get; set; }
        public byte[] EffectBytes { get; set; }

        public PlayerHealthEffectPacket() : base("", nameof(PlayerHealthEffectPacket))
        {
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Add = reader.ReadBoolean();
            EffectType = reader.ReadString();
            var effectLength = reader.ReadInt32();
            EffectBytes = reader.ReadBytes(effectLength);

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Add);
            writer.Write(Effect.GetType().Name);
            var effectBytesLength = 0;
            writer.Write(effectBytesLength);
            var preEffectBytesPosition = writer.BaseStream.Position;
            writer.Write(Effect.Id);
            writer.Write((byte)Effect.BodyPart);
            writer.Write((byte)Effect.State);
            //writer.Write(Effect.Single_0);
            writer.Write(Effect.DelayTime);
            writer.Write(Effect.BuildUpTime);
            writer.Write(Effect.WorkStateTime);
            writer.Write(Effect.ResidueStateTime);
            writer.Write(Effect.Strength);
            effectBytesLength = (int)writer.BaseStream.Position - (int)preEffectBytesPosition;
            writer.BaseStream.Position = preEffectBytesPosition;
            writer.Write(effectBytesLength);

            return ms.ToArray();    
        }


        public override void Process()
        {
            if (Method != nameof(PlayerHealthEffectPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            // TODO: Process of this packet

            if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
            {
                //client.ReceivedPackets.Enqueue(this);
                //var effect = AbstractEffect.Create(client.ActiveHealthController, this.Effect.ToString(), this.Effect.BodyPart, new EFT.Profile.ProfileHealth.RestoreInfo() { Time = this.Effect.TimeLeft });
            }


        }
    }
}
