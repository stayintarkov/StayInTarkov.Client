using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using StayInTarkov;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class AbstractHandsControllerDropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player.FirearmController);
            if (t == null)
                Logger.LogInfo($"AbstractHandsControllerDropPatch:Type is NULL");

            var method = ReflectionHelpers.GetMethodForType(t, "Drop");

            Logger.LogInfo($"AbstractHandsControllerDropPatch:{t.Name}:{method.Name}");
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance)
        {
            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPrefix]
        public static void PostPatch(EFT.Player.FirearmController __instance)
        {
            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("m", "CheckAmmo");
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
        }

        private static ConcurrentBag<long> ProcessedCalls = new();

        public static void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.Profile.AccountId, true);
                firearmCont.CheckAmmo();
            }
        }
    }
}
