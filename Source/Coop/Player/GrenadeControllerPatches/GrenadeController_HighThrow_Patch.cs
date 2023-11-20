using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.GrenadeControllerPatches
{
    internal class GrenadeController_HighThrow_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => null;//ReflectionHelpers.SearchForType("GrenadeController");
        public override string MethodName => "HighThrow";

        public GrenadeController_HighThrow_Patch()
        {
        }

        public GrenadeController_HighThrow_Patch(Type type)
        {
            OverrideInstanceType = type;
        }

        protected override MethodBase GetTargetMethod()
        {
            if (OverrideInstanceType == null)
            {
                var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName, false, true);
                return method;
            }
            else
            {
                var method = ReflectionHelpers.GetMethodForType(OverrideInstanceType, MethodName, false, true);
                return method;
            }
        }

        public static Dictionary<string, bool> CallLocally
            = new();

        /// <summary>
        /// Disable patch from starting with the others automatically
        /// </summary>
        public override bool DisablePatch => true;


        [PatchPrefix]
        public static bool PrePatch(
            object __instance,
            EFT.Player ____player
            )
        {
            if (____player == null)
            {
                Logger.LogError("Player property is NULL!");
                return false;
            }

            var result = false;
            if (CallLocally.TryGetValue(____player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            //Logger.LogDebug("GrenadeController_HighThrow_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(object __instance,
            EFT.Player ____player
            )
        {
            if (____player == null)
            {
                Logger.LogError("Player property could not be found!");
                return;
            }

            if (CallLocally.TryGetValue(____player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(____player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "rX", ____player.Rotation.x },
                { "rY", ____player.Rotation.y },
                { "m", "HighThrow" }
            };
            AkiBackendCommunicationCoop.PostLocalPlayerData(____player, dictionary);

            //Logger.LogDebug("GrenadeController_HighThrow_Patch:PostPatch");

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var playerHandsController = player.HandsController;
            //if (player.HandsController is EFT.Player.GrenadeController cont)
            //{
            //Logger.LogDebug("GrenadeController_HighThrow_Patch:Replicated");
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                {
                    var rX = float.Parse(dict["rX"].ToString());
                    var rY = float.Parse(dict["rY"].ToString());
                    Vector2 rot = new(rX, rY);
                    player.Rotation = rot;
                }
            }

            CallLocally.Add(player.Profile.AccountId, true);
            ReflectionHelpers.GetMethodForType(player.HandsController.GetType(), MethodName).Invoke(playerHandsController, new object[] { });
            //}
        }
    }
}
