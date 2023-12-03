//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.Proceed
//{
//    internal class Player_Proceed_EmptyHands_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player);

//        public override string MethodName => "ProceedEmptyHands";

//        public static List<string> CallLocally = new();

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "withNetwork");
//        }

//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player __instance)
//        {
//            return CallLocally.Contains(__instance.ProfileId);
//        }

//        [PatchPostfix]
//        public static void PostPatch(EFT.Player __instance, bool withNetwork, bool scheduled)
//        {
//            if (CallLocally.Contains(__instance.ProfileId))
//            {
//                CallLocally.Remove(__instance.ProfileId);
//                return;
//            }

//            PlayerProceedEmptyHandsPacket playerProceedEmptyHandsPacket = new(__instance.ProfileId, withNetwork, scheduled, "ProceedEmptyHands");
//            AkiBackendCommunication.Instance.SendDataToPool(playerProceedEmptyHandsPacket.Serialize());
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (!dict.ContainsKey("data"))
//                return;

//            PlayerProceedEmptyHandsPacket playerProceedEmptyHandsPacket = new(null, true, true, null);
//            playerProceedEmptyHandsPacket = playerProceedEmptyHandsPacket.DeserializePacketSIT(dict["data"].ToString());

//            if (HasProcessed(GetType(), player, playerProceedEmptyHandsPacket))
//                return;

//            CallLocally.Add(player.ProfileId);
//            player.Proceed(playerProceedEmptyHandsPacket.WithNetwork, null, playerProceedEmptyHandsPacket.Scheduled);
//        }
//    }

//    public class PlayerProceedEmptyHandsPacket : BasePlayerPacket
//    {
//        public bool WithNetwork { get; set; }

//        public bool Scheduled { get; set; }

//        public PlayerProceedEmptyHandsPacket(string profileId, bool withNetwork, bool scheduled, string method) : base(profileId, method)
//        {
//            WithNetwork = withNetwork;
//            Scheduled = scheduled;
//        }
//    }
//}