using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.KnifeControllerPatches
{
    internal class GrenadeController_ExamineWeapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.KnifeController);
        public override string MethodName => "KnifeController_ExamineWeapon";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "ExamineWeapon");
        }

        [PatchPostfix]
        public static void PostPatch(object __instance, EFT.Player ____player)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                coopPlayer.AddCommand(new GClass2142()
                {
                    Look = true
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
    }
}