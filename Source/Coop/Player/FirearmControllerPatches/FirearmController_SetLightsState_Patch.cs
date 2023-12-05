using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_SetLightsState_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "SetLightsState";

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(object __instance, LightsStates[] lightsStates, bool force, EFT.Player ____player)
        {
            return CallLocally.Contains(____player.ProfileId);
        }

        [PatchPostfix]
        public static void Postfix(object __instance, LightsStates[] lightsStates, bool force, EFT.Player ____player)
        {
            if (CallLocally.Contains(____player.ProfileId))
            {
                CallLocally.Remove(____player.ProfileId);
                return;
            }

            Dictionary<string, object> dict = new()
            {
                { "m", "SetLightsState" },
                { "lightsStates", lightsStates.ToJson() },
                { "force", force.ToString() }
            };
            AkiBackendCommunicationCoop.PostLocalPlayerData(____player, dict);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmController)
            {
                LightsStates[] lightsStates = dict["lightsStates"].ToString().SITParseJson<LightsStates[]>();
                bool force = bool.Parse(dict["force"].ToString());

                CallLocally.Add(player.ProfileId);
                firearmController.SetLightsState(lightsStates, force);

                lightsStates = null;
            }
        }
    }
}