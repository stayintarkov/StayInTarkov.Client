//using BepInEx.Logging;
//using StayInTarkov.Coop.NetworkPacket;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player
//{
//    internal class PlayerInventoryController_UnloadMagazine_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

//        public override string MethodName => "PlayerInventoryController_UnloadMagazine";

//        private ManualLogSource GetLogger()
//        {
//            return GetLogger(typeof(PlayerInventoryController_UnloadMagazine_Patch));
//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, "UnloadMagazine", false, true);
//        }

//        [PatchPrefix]
//        public static bool PrePatch()
//        {
//            return true;
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {

//        }

//    }
//}
