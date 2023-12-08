using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    public class Player_CreateCompass_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "CreateCompass";
        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var player = __instance;
            if (player == null)
            {
                return false;
            }

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void Postfix(EFT.Player __instance)
        {
            var player = __instance;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "m", "CreateCompass" }
            };

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
                player.CreateCompass();
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }
    }
}
