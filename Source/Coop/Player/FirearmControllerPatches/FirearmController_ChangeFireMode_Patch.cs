//using EFT.InventoryLogic;
//using Newtonsoft.Json;
//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.FirearmControllerPatches
//{
//    public class FirearmController_ChangeFireMode_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player.FirearmController);
//        public override string MethodName => "ChangeFireMode";
//        //public override bool DisablePatch => true;

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//            return method;
//        }

//        public static Dictionary<string, bool> CallLocally
//            = new();


//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
//        {
//            return true;
//        }

//        [PatchPostfix]
//        public static void PostPatch(
//            EFT.Player.FirearmController __instance
//            , Weapon.EFireMode fireMode
//            , EFT.Player ____player)
//        {
//            var player = ____player;
//            if (player == null)
//                return;

//            StayInTarkov.Coop.NetworkPacket.Player.Weapons.FireModePacket fireModePacket = new(____player.ProfileId, fireMode);
//            GameClient.SendData(fireModePacket.Serialize());

//        }

//        //private static List<long> ProcessedCalls = new List<long>();

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            StayInTarkov.Coop.NetworkPacket.Player.Weapons.FireModePacket fmp = new(player.ProfileId, Weapon.EFireMode.single);

//            if (dict.ContainsKey("data"))
//                fmp.Deserialize((byte[])dict["data"]);

//            if (player.HandsController is EFT.Player.FirearmController firearmCont)
//            {
//                try
//                {
//                    //if (Enum.TryParse<Weapon.EFireMode>(dict["f"].ToString(), out var firemode))
//                    var firemode = (Weapon.EFireMode)fmp.FireMode;
//                    {
//                        //Logger.LogInfo("Replicated: Calling Change FireMode");
//                        firearmCont.CurrentOperation.ChangeFireMode(firemode);
//                    }
//                }
//                catch (Exception e)
//                {
//                    Logger.LogInfo(e);
//                }
//            }
//        }

        
//    }
//}
