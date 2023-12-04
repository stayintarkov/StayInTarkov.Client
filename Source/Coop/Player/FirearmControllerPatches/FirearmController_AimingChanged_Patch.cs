//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.FirearmControllerPatches
//{
//    internal class FirearmController_AimingChanged_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player.FirearmController);
//        public override string MethodName => "AimingChanged";

//        [PatchPostfix]
//        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player)
//        {
//            var coopPlayer = ____player as CoopPlayer;
//            if (coopPlayer != null)
//            {
//                coopPlayer.AddCommand(new GClass2134()
//                {
//                    IsAiming = __instance.IsAiming,
//                    AimingIndex = (__instance.IsAiming ? __instance.Weapon.AimIndex.Value : 0)
//                });
//            }
//            else
//            {
//                Logger.LogError("No CoopPlayer found!");
//            }
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            return;
//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//        }
//    }
//}