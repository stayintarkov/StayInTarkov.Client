//using EFT;
//using EFT.NextObservedPlayer;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;

///*
// * Developer note from Lacyway
// * This is a workaround for the bug where the player gets stuck sliding between running/idle for an unknown reason 
// */

//namespace StayInTarkov.Coop.GameWorldZ
//{
//    internal class StateContext_OnStateEnter_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(StateContext);
//        public override string MethodName => "OnStateEnter";

//        [PatchPrefix]
//        public static bool Postfix(StateContext __instance, BaseMovementState nextState, ref GClass1641 ___gclass1641_0, ref EPlayerState ___eplayerState_0, ref EPlayerState ___eplayerState_1,
//            Action<EPlayerState, EPlayerState> ___action_0, Dictionary<EPlayerState, GClass1641> ___dictionary_0)
//        {
//            bool run = false;

//            if (nextState.Name == EPlayerState.Idle && __instance.ActualMotion.x > 0.01f)
//            {
//                run = true;
//            }
//            else if (___gclass1641_0 == nextState)
//            {
//                return false;
//            }

//            //if (nextState.Name == EPlayerState.Run)
//            //{
//            //    run = true;
//            //    nextState = ___dictionary_0.ElementAt(0).Value;
//            //}

//            ___eplayerState_0 = ___gclass1641_0.Name;
//            ___gclass1641_0.Exit(false);
//            ___gclass1641_0 = (nextState as GClass1641);
//            ___eplayerState_1 = ___gclass1641_0.Name;
//            ___gclass1641_0.Enter(false);
//            Action<EPlayerState, EPlayerState> action = ___action_0;
//            if (action == null)
//            {
//                return false;
//            }

//            if (run == true)
//            {
//                Logger.LogInfo("RunAction");
//                action(___eplayerState_0, EPlayerState.Run);
//            }
//            else
//            {
//                Logger.LogInfo("NotRunAction");
//                action(___eplayerState_0, ___gclass1641_0.Name);
//            }

//            return false;
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

//// ???
////if (nextState.Name == EPlayerState.Run)
////    shouldRun = false;