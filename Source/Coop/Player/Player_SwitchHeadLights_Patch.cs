using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    internal class Player_SwitchHeadLights_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.LocalPlayer);

        public override string MethodName => "SwitchHeadLights";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            if (CallLocally.Contains(__instance.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PatchPostfix(EFT.Player __instance, bool togglesActive, bool changesState)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "t", DateTime.Now.Ticks.ToString("G") },
                { "m", "SwitchHeadLights" },
                { "togglesActive", togglesActive.ToString() },
                { "changesState", changesState.ToString() }
            };
            AkiBackendCommunicationCoop.PostLocalPlayerData(__instance, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var togglesActive = bool.Parse(dict["togglesActive"].ToString());
            var changesState = bool.Parse(dict["changesState"].ToString());

            CallLocally.Add(player.ProfileId);
            Logger.LogDebug($"Player_SwitchHeadLights_Patch:Replicated. Calling SwitchHeadLights(togglesActive: {togglesActive}, changesState: {changesState})");
            player.SwitchHeadLights(togglesActive, changesState);
        }
    }
}
