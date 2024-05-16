using EFT.HealthSystem;
using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GClass2428<EFT.HealthSystem.ActiveHealthController.GClass2427>;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public class RestoreBodyPartPacket : BasePlayerPacket
    {
        public string BodyPart { get; set; }
        public float HealthPenalty { get; set; }

        public RestoreBodyPartPacket() : base("", nameof(RestoreBodyPartPacket)) { }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(BodyPart);
            writer.Write(HealthPenalty);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            BodyPart = reader.ReadString();
            HealthPenalty = reader.ReadSingle();
            return this;
        }

        public override void Process()
        {
            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            if (!coopGameComponent.Players.ContainsKey(ProfileId))
                return;

            var player = coopGameComponent.Players[ProfileId];

            if (player.HealthController != null && player.HealthController.IsAlive)
            {
                StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}:Replicated: Calling RestoreBodyPart");

                var bodyPart = (EBodyPart)Enum.Parse(typeof(EBodyPart), BodyPart, true);
                var bodyPartState = GetBodyPartDictionary(player)[bodyPart];

                if (bodyPartState == null)
                {
                    StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}: Could not retrieve {player.ProfileId}'s Health State for Body Part {bodyPart}");
                    return;
                }

                if (bodyPartState.IsDestroyed)
                {
                    bodyPartState.IsDestroyed = false;
                    var healthPenalty = HealthPenalty + (1f - HealthPenalty) * player.Skills.SurgeryReducePenalty;
                    StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}:HealthPenalty::" + healthPenalty);
                    bodyPartState.Health = new HealthValue(1f, Mathf.Max(1f, Mathf.Ceil(bodyPartState.Health.Maximum * healthPenalty)), 0f);

                    player.ExecuteSkill(new Action<float>(player.Skills.SurgeryAction.Complete));
                    player.UpdateSpeedLimitByHealth();
                }
            }
        }

        private Dictionary<EBodyPart, BodyPartState> GetBodyPartDictionary(EFT.Player player)
        {
            try
            {
                var bodyPartDict
                = ReflectionHelpers.GetFieldOrPropertyFromInstance<Dictionary<EBodyPart, BodyPartState>>
                (player.PlayerHealthController, "Dictionary_0", false);
                if (bodyPartDict == null)
                {
                    StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}:Could not retrieve {player.ProfileId}'s Health State Dictionary");
                    return null;
                }

                return bodyPartDict;
            }
            catch (Exception ex)
            {
                StayInTarkovHelperConstants.Logger.LogError($"{nameof(PlayerInformationPacket)}: {ex}");
            }

            return null;
        }
    }
}
