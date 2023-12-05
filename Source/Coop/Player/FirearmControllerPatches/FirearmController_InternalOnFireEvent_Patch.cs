//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.FirearmControllerPatches
//{
//    internal class FirearmController_InternalOnFireEvent_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player.FirearmController);
//        public override string MethodName => "InternalOnFireEvent";

//        [PatchPostfix]
//        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player, EFT.InventoryLogic.Weapon ___weapon_0)
//        {
//            var coopPlayer = ____player as CoopPlayer;
//            EFT.InventoryLogic.Weapon.EMalfunctionState emalfunctionState;
//            emalfunctionState = ___weapon_0.MalfState;

//            if (coopPlayer != null)
//            {
//                if (emalfunctionState != EFT.InventoryLogic.Weapon.EMalfunctionState.None)
//                {
//                    coopPlayer.AddCommand(new GClass2150()
//                    {
//                        MalfunctionState = emalfunctionState
//                    });
//                }
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