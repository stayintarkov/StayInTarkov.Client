//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player
//{

//    internal class Player_Jump_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player);
//        public override string MethodName => "Jump";

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
//            return method;
//        }

//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player __instance)
//        {
//            if (IsHighPingOrAI(__instance))
//                return true;

//            return false;
//        }

//        [PatchPostfix]
//        public static void PostPatch(
//           EFT.Player __instance
//            )
//        {

//            // ---------------------------------------------------------------------------------------------------------------------
//            // Note. As this patch calls a different method to "replicate", you do not need to do any trickery to stop the loop here

//            var player = __instance;

//            BasePlayerPacket playerPacket = new(player.ProfileId, "Jump");
//            var serialized = playerPacket.Serialize();
//            AkiBackendCommunication.Instance.SendDataToPool(serialized);
//            //AkiBackendCommunication.Instance.PostDownWebSocketImmediately( serialized );

//            //Logger.LogInfo("================================");
//            //Logger.LogInfo("Jump:PostPatch");
//            //Logger.LogInfo(serialized);
//            //Logger.LogInfo("================================");

//            // ---------------------------------------------------------------------------------------------------------------------
//            // Note. If the player is AI or High Ping. Stop the double jump caused by the sent packet above
//            //if (IsHighPingOrAI(player))
//            //{
//            //    HasProcessed(typeof(Player_Jump_Patch), player, playerPacket);
//            //}
//        }


//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            GetLogger(typeof(Player_Jump_Patch)).LogDebug("Jump:Replicated");

//            if (AkiBackendCommunication.Instance.HighPingMode && player.IsYourPlayer)
//            {
//                return;
//            }

//            BasePlayerPacket bpp = new();
//            bpp.DeserializePacketSIT(dict["data"].ToString());

//            if (HasProcessed(GetType(), player, bpp))
//                return;

//            try
//            {
//                player.CurrentManagedState.Jump();
//            }
//            catch (Exception e)
//            {
//                Logger.LogInfo(e);
//            }

//        }

//    }
//}
