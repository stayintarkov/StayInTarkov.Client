using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.AI
{
    internal class BotsGroup_CheckAndAddEnemy_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(BotsGroup);

        public override string MethodName => "CheckAndAddEnemy";

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPrefix]
        public static bool PrePatch(BotsGroup __instance, IAIDetails player, bool ignoreAI = false)
        {
            if (player.AIData != null)
                return false;
            
            return true;
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }
    }
}
