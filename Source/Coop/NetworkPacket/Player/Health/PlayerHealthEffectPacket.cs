using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers;
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

        public string EffectType { get; set; }

        public EBodyPart BodyPart { get; set; }

        public float TimeLeft { get; set; }

        public PlayerHealthEffectPacket() : base("", nameof(PlayerHealthEffectPacket))
        {
        }

        public PlayerHealthEffectPacket(string profileId, bool addEffect, string effectType, EBodyPart bodyPart, float timeLeft) : base(new string(profileId.ToCharArray()), nameof(PlayerHealthEffectPacket))
        {
            EffectType = effectType;
            BodyPart = bodyPart;
            TimeLeft = timeLeft;
            Add = addEffect;
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Add = reader.ReadBoolean();
            EffectType = reader.ReadString();
            BodyPart = (EBodyPart)reader.ReadByte();
            TimeLeft = reader.ReadSingle();

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Add);
            writer.Write(EffectType);
            writer.Write((byte)BodyPart);
            writer.Write(TimeLeft);

            return ms.ToArray();    
        }


        public override void Process()
        {
            if (Method != nameof(PlayerHealthEffectPacket))
                return;

            //StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            //if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            //{
            //    StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Process)}:Unable to obtain CoopGameComponent");
            //    return;
            //}

            //if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
            //{
            //    StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}:Creating Effect");

            //    //client.ReceivedPackets.Enqueue(this);
            //    var effect = AbstractEffect.Create(client.ActiveHealthController, this.EffectType, this.BodyPart, new EFT.Profile.ProfileHealth.RestoreInfo() { Time = this.TimeLeft });

            //    StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}:client.ActiveHealthController is {client.ActiveHealthController.GetType()}");

            //    var coopHealthController = client.ActiveHealthController as CoopHealthControllerClient;
            //    if (coopHealthController != null) 
            //    { 
            //        coopHealthController.ReceiveEffect(effect);
            //    }

            //    StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}:client.PlayerHealthController is {client.PlayerHealthController.GetType()}");

            //    coopHealthController = client.PlayerHealthController as CoopHealthControllerClient;
            //    if (coopHealthController != null)
            //    {
            //        coopHealthController.ReceiveEffect(effect);
            //    }
            //}
            //else
            //{
            //    StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Process)}:Unable to find player {ProfileId}");
            //}


        }
    }
}
