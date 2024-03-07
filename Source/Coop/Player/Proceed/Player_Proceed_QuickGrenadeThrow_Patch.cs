//using Comfort.Common;
//using EFT.InventoryLogic;
//using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace StayInTarkov.Coop.Player.Proceed
//{
//    internal class Player_Proceed_QuickGrenadeThrow_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player);

//        public override string MethodName => "ProceedQuickGrenadeThrow";

//        public static List<string> CallLocally = new();

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed"
//                   && x.GetParameters().Length == 3
//                   && x.GetParameters()[0].Name == "throwWeap"
//                   && x.GetParameters()[1].Name == "callback"
//                   && x.GetParameters()[2].Name == "scheduled"
//                   && x.GetParameters()[1].ParameterType == typeof(Callback<IGrenadeQuickUseController>));
//        }

//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player __instance)
//        {
//            return true; // Only AI use QuickGrenadeThrow.
//        }

//        [PatchPostfix]
//        public static void PostPatch(EFT.Player __instance, GrenadeClass throwWeap, bool scheduled)
//        {
//            if (CallLocally.Contains(__instance.ProfileId))
//            {
//                CallLocally.Remove(__instance.ProfileId);
//                return;
//            }

//            PlayerProceedPacket playerProceedPacket = new(__instance.ProfileId, throwWeap.Id, throwWeap.TemplateId, scheduled, "ProceedQuickGrenadeThrow");
//            GameClient.SendData(playerProceedPacket.Serialize());
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (!dict.ContainsKey("data"))
//                return;

//            PlayerProceedPacket playerProceedPacket = new(player.ProfileId, null, null, true, null);
//            playerProceedPacket.Deserialize((byte[])dict["data"]);

//            if (HasProcessed(GetType(), player, playerProceedPacket))
//                return;

//            if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
//            {
//                if (item is GrenadeClass throwWeap)
//                {
//                    CallLocally.Add(player.ProfileId);
//                    player.Proceed(throwWeap, (Callback<IGrenadeQuickUseController>)null, playerProceedPacket.Scheduled);
//                }
//                else
//                {
//                    Logger.LogError($"Player_Proceed_QuickGrenadeThrow_Patch:Replicated. Item {playerProceedPacket.ItemId} is not a GrenadeClass!");
//                }
//            }
//            else
//            {
//                Logger.LogError($"Player_Proceed_QuickGrenadeThrow_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");
//            }
//        }
//    }
//}