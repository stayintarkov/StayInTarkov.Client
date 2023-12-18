using EFT.HealthSystem;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health
{
    internal class ActiveHealthController_AddEffect_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ActiveHealthController.AbstractEffect);

        public override string MethodName => "Removed";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PatchPostfix(ActiveHealthController.AbstractEffect __instance)
        {
            //var player = health.Player as CoopPlayer;
            var player = __instance.HealthController.Player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            var effectType = __instance.GetType().Name;

            //EFT.UI.ConsoleScreen.Log($"AbstractEffect::Removed::PostPatch: {__instance.Id} {effectType} {__instance.BuildUpTime}");

            player.HealthPacket.HasRemoveEffect = true;
            player.HealthPacket.RemoveEffectPacket = new()
            {
                Id = __instance.Id,
                Type = effectType,
                BodyPartType = __instance.BodyPart
            };
            player.HealthPacket.ToggleSend();
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
