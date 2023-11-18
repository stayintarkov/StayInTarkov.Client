using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    internal class Player_Say_Patch : ModuleReplicationPatch
    {
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Say";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.Contains(__instance.ProfileId))
                result = true;

            return result;
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
            var player = __instance;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("event", @event);
            dictionary.Add("demand", demand.ToString());
            dictionary.Add("delay", delay.ToString());
            dictionary.Add("mask", mask);
            dictionary.Add("probability", probability.ToString());
            dictionary.Add("aggressive", aggressive.ToString());
            dictionary.Add("m", "Say");
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;

            try
            {
                CallLocally.Add(player.ProfileId);
                player.Say(
                    (EPhraseTrigger)Enum.Parse(typeof(EPhraseTrigger), dict["event"].ToString())
                    , demand: bool.Parse(dict["demand"].ToString())
                    , delay: float.Parse(dict["delay"].ToString())
                    , mask: (ETagStatus)Enum.Parse(typeof(ETagStatus), dict["mask"].ToString())
                    , probability: int.Parse(dict["probability"].ToString())
                    , aggressive: bool.Parse(dict["aggressive"].ToString())

                    );
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }

        }
    }
}
