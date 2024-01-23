using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.GrenadeControllerPatches
{
    internal class GrenadeController_PullRingForHighThrow_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.GrenadeController);

        public override string MethodName => "PullRingForHighThrow";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(object __instance, EFT.Player ____player)
        {
            return CallLocally.Contains(____player.ProfileId);
        }

        [PatchPostfix]
        public static void PostPatch(object __instance, EFT.Player ____player)
        {
            if (CallLocally.Contains(____player.ProfileId))
            {
                CallLocally.Remove(____player.ProfileId);
                return;
            }

            GameClient.SendData(new GrenadeThrowPacket(____player.ProfileId, ____player.Rotation, "PullRingForHighThrow").Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            GrenadeThrowPacket grenadeThrowPacket = new(null, Vector2.zero, null);
            grenadeThrowPacket.Deserialize((byte[])dict["data"]);

            if (HasProcessed(GetType(), player, grenadeThrowPacket))
                return;

            if (player.TryGetComponent(out PlayerReplicatedComponent prc) && prc.IsClientDrone)
                player.Rotation = new Vector2(grenadeThrowPacket.rX, grenadeThrowPacket.rY);

            if (player.HandsController is EFT.Player.GrenadeController grenadeController)
            {
                CallLocally.Add(player.ProfileId);
                grenadeController.PullRingForHighThrow();
            }
        }
    }
}