using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_SetScopeMode_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "SetScopeMode";

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(object __instance, ScopeStates[] scopeStates, EFT.Player ____player)
        {
            return CallLocally.Contains(____player.ProfileId);
        }

        [PatchPostfix]
        public static void Postfix(object __instance, ScopeStates[] scopeStates, EFT.Player ____player)
        {
            if (CallLocally.Contains(____player.ProfileId))
            {
                CallLocally.Remove(____player.ProfileId);
                return;
            }

            Dictionary<string, object> dict = new()
            {
                { "m", "SetScopeMode" },
                { "scopeStates", scopeStates.ToJson() }
            };
            AkiBackendCommunicationCoop.PostLocalPlayerData(____player, dict);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmController)
            {
                ScopeStates[] scopeStates = dict["scopeStates"].ToString().SITParseJson<ScopeStates[]>();

                CallLocally.Add(player.ProfileId);
                firearmController.SetScopeMode(scopeStates);

                scopeStates = null;
            }
        }
    }
}