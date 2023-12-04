using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_InitiateShot_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "InitiateShot";

        [PatchPostfix]
        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player, int chamberIndex, Vector3 shotPosition, Vector3 shotDirection)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                coopPlayer.AddCommand(new GClass2123()
                {
                    AmmoTemplate = __instance.Weapon.CurrentAmmoTemplate._id,
                    ChamberIndex = chamberIndex,
                    ShotDirection = shotDirection,
                    ShotPosition = shotPosition
                });
            }
            else
            {
                Logger.LogError("No CoopPlayer found!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }
    }
}