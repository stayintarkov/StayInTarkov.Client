using EFT.HealthSystem;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        //[PatchPrefix]
        //public static bool PrePatch(EFT.Player __instance)
        //{
        //    //Logger.LogDebug("RestoreBodyPartPatch:PrePatch");
        //    var result = false;
        //    return true;
        //}

        [PatchPostfix]
        public static void PatchPostfix(PlayerHealthController __instance, EBodyPart bodyPart, float healthPenalty)
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            var player = __instance.Player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            player.HealthPacket.HasBodyPartRestoreInfo = true;
            player.HealthPacket.RestoreBodyPartPacket = new()
            {
                BodyPartType = bodyPart,
                HealthPenalty = healthPenalty
            };
            player.HealthPacket.ToggleSend();

            // If it is a client Drone, do not resend the packet again!
            //if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            //{
            //    if (prc.IsClientDrone)
            //        return;
            //}


            //RestoreBodyPartPacket restoreBodyPartPacket = new();
            //restoreBodyPartPacket.ProfileId = player.ProfileId;
            //restoreBodyPartPacket.BodyPart = bodyPart.ToString();
            //restoreBodyPartPacket.HealthPenalty = healthPenalty;
            ////var json = restoreBodyPartPacket.ToJson();
            ////Logger.LogInfo(json);
            //AkiBackendCommunication.Instance.SendDataToPool(restoreBodyPartPacket.Serialize());
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }

        //protected sealed class BodyPartState
        //{
        //    public bool IsDestroyed;

        //    public HealthValue Health;
        //}
    }
}
