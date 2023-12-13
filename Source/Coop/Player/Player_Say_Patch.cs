using EFT;
using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    internal class Player_Say_Patch : ModuleReplicationPatch
    {
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Say";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
            EPhraseTrigger @event
            , bool demand
            , float delay
            , ETagStatus mask
            , int probability
            , bool aggressive
            )
        {
            
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            ((CoopPlayer)player).ReceiveSay((EPhraseTrigger)Enum.Parse(typeof(EPhraseTrigger), dict["event"].ToString()), int.Parse(dict["index"].ToString()));
        }
    }
}
