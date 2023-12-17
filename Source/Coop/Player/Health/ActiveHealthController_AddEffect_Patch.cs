using EFT.HealthSystem;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health
{
    internal class ActiveHealthController_AddEffect_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ActiveHealthController);

        public override string MethodName => "AddEffect";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PatchPostfix(ActiveHealthController __instance, ref ActiveHealthController.AbstractEffect __result,
            EBodyPart bodyPart, float? delayTime = null,
            float? workTime = null, float? residueTime = null,
            float? strength = null)
        {
            var player = __instance.Player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            var effectType = GClass2333.GClass2334.Make(__result);

            EFT.UI.ConsoleScreen.Log($"AddEffect::PostPatch: {__result.Id} {effectType} {bodyPart} {delayTime} {__result.BuildUpTime} {workTime} {residueTime} {strength}");

            player.HealthPacket.HasAddEffect = true;
            player.HealthPacket.AddEffectPacket = new()
            {
                Id = __result.Id,
                Type = effectType,
                BodyPartType = bodyPart,
                DelayTime = delayTime,
                BuildUpTime = __result.BuildUpTime,
                WorkTime = workTime,
                ResidueTime = residueTime,
                Strength = strength
            };
            player.HealthPacket.ToggleSend();
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
