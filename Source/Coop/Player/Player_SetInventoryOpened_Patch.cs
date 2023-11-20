using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    public class Player_SetInventoryOpened_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "SetInventoryOpened";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

            return method;
        }

        public static HashSet<string> CallLocally
            = new();

        [PatchPrefix]
        public static bool PrePatch(ref EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.Contains(__instance.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           ref EFT.Player __instance,
            ref bool opened
            )
        {
            var player = __instance;

            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("o", opened.ToString());
            dictionary.Add("m", "SetInventoryOpened");
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
            //dictionary.Clear();
            //dictionary = null;
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
                var opened = Convert.ToBoolean(dict["o"].ToString());
                player.SetInventoryOpened(opened);
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}

