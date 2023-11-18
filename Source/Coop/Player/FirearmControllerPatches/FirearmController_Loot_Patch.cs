using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_Loot_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "Loot";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName, findFirst: true);
            return method;
        }

        public static List<string> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, ref bool p)
        {
            GetLogger(typeof(FirearmController_Loot_Patch)).LogInfo("Loot");

            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("p", p);
            dictionary.Add("m", "Loot");
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.ProfileId);
                var p = bool.Parse(dict["p"].ToString());
                firearmCont.Loot(p);
            }
        }
    }
}
