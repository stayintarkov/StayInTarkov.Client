﻿using EFT.HealthSystem;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static AHealthController<EFT.HealthSystem.ActiveHealthController.AbstractHealthEffect>;

namespace StayInTarkov.Coop.Player.Health
{
    internal class RestoreBodyPartPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(PlayerHealthController);

        public override string MethodName => "RestoreBodyPart";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PrePatch");
            var result = false;
            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EBodyPart bodyPart
            , float healthPenalty
            )
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            var player = __instance.Player;

            // If it is a client Drone, do not resend the packet again!
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }


            RestoreBodyPartPacket restoreBodyPartPacket = new()
            {
                ProfileId = player.ProfileId,
                BodyPart = bodyPart.ToString(),
                HealthPenalty = healthPenalty
            };
            //var json = restoreBodyPartPacket.ToJson();
            //Logger.LogInfo(json);
            AkiBackendCommunication.Instance.SendDataToPool(restoreBodyPartPacket.Serialize());
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            RestoreBodyPartPacket restoreBodyPartPacket = new();
            restoreBodyPartPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, restoreBodyPartPacket))
                return;

            if (player.HealthController != null && player.HealthController.IsAlive)
            {
                Logger.LogDebug("Replicated: Calling RestoreBodyPart");
                Logger.LogDebug(restoreBodyPartPacket.ToJson());

                if (dict == null)
                {
                    Logger.LogError($"Dictionary packet is null?");
                    return;

                }
                //Logger.LogInfo(dict.ToJson());

                var bodyPart = (EBodyPart)Enum.Parse(typeof(EBodyPart), restoreBodyPartPacket.BodyPart, true);
                var bodyPartState = GetBodyPartDictionary(player)[bodyPart];

                if (bodyPartState == null)
                {
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Health State for Body Part {restoreBodyPartPacket.BodyPart}");
                    return;
                }

                if (bodyPartState.IsDestroyed)
                {
                    bodyPartState.IsDestroyed = false;
                    var healthPenalty = restoreBodyPartPacket.HealthPenalty + (1f - restoreBodyPartPacket.HealthPenalty) * player.Skills.SurgeryReducePenalty;
                    Logger.LogDebug("RestoreBodyPart::HealthPenalty::" + healthPenalty);
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
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Health State Dictionary");
                    return null;
                }
                //Logger.LogInfo(bodyPartDict.ToJson());
                return bodyPartDict;
            }
            catch (Exception)
            {

                var field = ReflectionHelpers.GetFieldFromType(player.PlayerHealthController.GetType(), "Dictionary_0");
                Logger.LogError(field);
                var type = field.DeclaringType;
                Logger.LogError(type);
                var val = field.GetValue(player.PlayerHealthController);
                Logger.LogError(val);
                var valType = field.GetValue(player.PlayerHealthController).GetType();
                Logger.LogError(valType);
            }

            return null;
        }

        public class RestoreBodyPartPacket : BasePlayerPacket
        {
            public string BodyPart { get; set; }
            public float HealthPenalty { get; set; }

            public RestoreBodyPartPacket() : base()
            {
                Method = "RestoreBodyPart";
            }
        }

        //protected sealed class BodyPartState
        //{
        //    public bool IsDestroyed;

        //    public HealthValue Health;
        //}
    }
}
