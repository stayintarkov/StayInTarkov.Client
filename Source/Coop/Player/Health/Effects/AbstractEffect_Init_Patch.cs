using EFT.HealthSystem;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health.Effects
{
    internal class AbstractEffect_Init_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ActiveHealthController.AbstractEffect);

        public override string MethodName => "Init";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PatchPostfix(ActiveHealthController.AbstractEffect __instance, ActiveHealthController health, EBodyPart bodyPart,
            float? delayTime, float? workTime, float? residueTime, float strength, int? updateTime)
        {
            var player = health.Player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && !MatchmakerAcceptPatches.IsServer && !player.IsAI)
                return;

            var effectType = __instance.GetType().Name;

            //EFT.UI.ConsoleScreen.Log($"AbstractEffect::Init::PostPatch: {__instance.Id} {effectType} {bodyPart} {delayTime} {__instance.BuildUpTime} {workTime} {residueTime} {strength}");

            player.HealthPacket.HasAddEffect = true;
            player.HealthPacket.AddEffectPacket = new()
            {
                Id = __instance.Id,
                Type = effectType,
                BodyPartType = bodyPart,
                DelayTime = delayTime == null ? 0 : (float)delayTime,
                BuildUpTime = __instance.BuildUpTime,
                WorkTime = workTime == null ? 0 : (float)workTime,
                ResidueTime = residueTime == null ? 0 : (float)residueTime,
                Strength = strength
            };
            player.HealthPacket.ToggleSend();
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
