//using Comfort.Common;
//using EFT;
//using EFT.Counters;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.AI
//{
//    internal class BotsGroup_AddEnemy_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(BotsGroup);

//        public override string MethodName => "AddEnemy";

//        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

//        [PatchPrefix]
//        public static bool PrePatch(BotsGroup __instance, ref IAIDetails person, EBotEnemyCause cause, Dictionary<IAIDetails, BotSettingsClass> ___Enemies)
//        {
//            return true;  
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            return;
//        }
//    }
//}
