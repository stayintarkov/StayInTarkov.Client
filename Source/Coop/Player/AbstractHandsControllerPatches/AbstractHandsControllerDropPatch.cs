//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.FirearmControllerPatches
//{
//    internal class AbstractHandsControllerDropPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            var t = typeof(EFT.Player.FirearmController);
//            if (t == null)
//                Logger.LogInfo($"AbstractHandsControllerDropPatch:Type is NULL");

//            var method = ReflectionHelpers.GetMethodForType(t, "Drop");

//            return method;
//        }

//        [PatchPrefix]
//        public static void PostPatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
//        {
//            var coopPlayer = ____player as CoopPlayer;
//            if (coopPlayer != null)
//            {
//                coopPlayer.AddCommand(new GClass2130()
//                {
                    
//                });
//            }
//        }

//        public static void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            return;
//        }
//    }
//}
