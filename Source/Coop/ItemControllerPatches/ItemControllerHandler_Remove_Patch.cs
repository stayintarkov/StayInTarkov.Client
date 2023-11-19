//using BepInEx.Logging;
//using Comfort.Common;
//using EFT;
//using EFT.InventoryLogic;
//using SIT.Coop.Core.Web;
//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.InteropServices.WindowsRuntime;

//namespace SIT.Core.Coop.ItemControllerPatches
//{
//    internal class ItemControllerHandler_Remove_Patch : ModuleReplicationPatch
//    {
//        public static ManualLogSource Log { get {

//                return GetLogger(typeof(ItemControllerHandler_Remove_Patch));
            
//            } }
//        public override Type InstanceType => typeof(ItemMovementHandler);

//        public override string MethodName => "IC_Remove";

//        public static List<string> CallLocally = new();

//        public static List<string> DisableForId = new();

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            var icId = dict["icId"].ToString();
//            Log.LogInfo(icId);
//            var icCId = dict["icCId"].ToString();
//            Log.LogInfo(icCId);


//            if (DisableForId.Contains(icId))
//            {
//                Log.LogDebug("Not receiving item move for replication. Currently Disabled.");
//                return;
//            }

//            if (!ItemFinder.TryFindItem(dict["id"].ToString(), out Item item))
//                return;


//            var itemController = Singleton<GameWorld>.Instance.FindControllerById(icId);
//            if(itemController == null)
//            {
//                Log.LogError($"Unable to find ItemController for Id {icId}");
//                return;
//            }

//            //if (CallLocally.Contains(icId))
//            //    return;

//            //CallLocally.Add(icId);
//            //ItemMovementHandler.Remove(item, itemController, false, false);
//            //CallLocally.Remove(icId);

//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, "Move");
//        }

//        [PatchPrefix]
//        public static bool Prefix(
//            //object __instance,
//            Item item
//            , ItemAddress to
//            , ItemController itemController
//            , bool simulate = false
//            )
//        {
//            if (simulate)
//                return true;

//            return true;
//        }

//        [PatchPostfix]
//        public static void Postfix(
//            object __instance,
//            Item item
//            , ItemController itemController
//            , bool simulate = false
//            )
//        {
//            if (simulate)
//                return;

//            CoopGameComponent coopGameComponent = null;

//            if (!CoopGameComponent.TryGetCoopGameComponent(out coopGameComponent))
//                return;

//            //var inventoryController = itemController as EFT.Player.PlayerInventoryController;
//            //if (inventoryController == null)
//            //{
//            //    Log.LogError("TODO: FIXME: ItemController isn't a Player Inventory Controller. Derp!");
//            //    return;
//            //}

//            //if (!coopGameComponent.Players.Any(x => x.Key == inventoryController.Profile.ProfileId))
//            //{
//            //    Log.LogError($"Unable to find player of Id {inventoryController.Profile.ProfileId} in Raid.");
//            //    return;
//            //}

//            if(CallLocally.Contains(itemController.ID))
//            {
//                CallLocally.Remove(itemController.ID);
//                return;
//            }    

//            // The player chosen here is not needed and a hack to bypass CoopGameController checks. This is using Controller Ids.
//            //var player = coopGameComponent.Players.First(x => x.Key == inventoryController.Profile.ProfileId).Value;
//            var player = coopGameComponent.Players.First().Value;

//            Dictionary<string, object> dictionary = new()
//            {
//                { "t", DateTime.Now.Ticks.ToString("G") }
//            };

//            dictionary.Add("id", item.Id);
//            dictionary.Add("tpl", item.TemplateId);
//            dictionary.Add("icId", itemController.ID);
//            dictionary.Add("icCId", itemController.CurrentId);
//            dictionary.Add("m", "IC_Remove");

//            //Logger.LogInfo(dictionary.ToJson());
//            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);

//        }

//    }
//}
