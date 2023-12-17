//using EFT.HealthSystem;
//using StayInTarkov.Coop.Matchmaker;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.Health
//{
//    internal class ActiveHealthController_DoBleed_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(ActiveHealthController);

//        public override string MethodName => "DoBleed";

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(ActiveHealthController __instance, EBodyPart bodyPart)
//        {
//            var player = __instance.Player as CoopPlayer;
//            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
//                return;

//            player.HealthPacket.HasAddEffect = true;
//            player.HealthPacket.AddEffectPacket = new()
//            {
//                EffectTypeValue = Networking.SITSerialization.AddEffectPacket.EffectType.Bleed,
//                BodyPartType = bodyPart
//            };
//            player.HealthPacket.ToggleSend();
//        }


//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {

//        }
//    }
//}
