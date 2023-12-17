using EFT.HealthSystem;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health
{
    internal class ActiveHealthController_DoPainKiller_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ActiveHealthController);

        public override string MethodName => "DoPainKiller";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PatchPostfix(ActiveHealthController __instance)
        {
            var player = __instance.Player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            

            player.HealthPacket.HasAddEffect = true;
            player.HealthPacket.AddEffectPacket = new()
            {
                EffectTypeValue = Networking.SITSerialization.AddEffectPacket.EffectType.PainKiller
            };
            player.HealthPacket.ToggleSend();

            EFT.UI.ConsoleScreen.Log($"{player.HealthPacket.HasAddEffect} {player.HealthPacket.AddEffectPacket.EffectTypeValue}");
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
