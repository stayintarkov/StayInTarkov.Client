using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.ItemHandsControllerPatches
{
    internal class ItemHandsController_Pickup_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.ItemHandsController);
        public override string MethodName => "Pickup";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.ItemHandsController __instance)
        {
            //var player = ____player;
            var player = ReflectionHelpers.GetFieldFromType(__instance.GetType(), "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return false;

            //var result = false;
            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            Logger.LogDebug("ItemHandsControllerPickupPatch: PrePatch");
            //return result;
            return true;
        }

        //        [PatchPostfix]
        //        public static void PostPatch(EFT.Player.ItemHandsController __instance, bool p)
        //        {
        //            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
        //            if (player == null)
        //                return;

        //            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
        //            {
        //                CallLocally.Remove(player.Profile.AccountId);
        //                return;
        //            }

        //            Dictionary<string, object> dictionary = new Dictionary<string, object>();
        //            dictionary.Add("t", DateTime.Now.Ticks);
        //            dictionary.Add("pick", p.ToString());
        //            dictionary.Add("m", "Pickup");
        //            ServerCommunication.PostLocalPlayerData(player, dictionary);
        //        }

        //        private static ConcurrentBag<long> ProcessedCalls = new ConcurrentBag<long>();

        //        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        //        {
        //            var timestamp = long.Parse(dict["t"].ToString());
        //            if (!ProcessedCalls.Contains(timestamp))
        //                ProcessedCalls.Add(timestamp);
        //            else
        //                return;

        //            if (player.HandsController is EFT.Player.ItemHandsController itemhandscont)
        //            {
        //                CallLocally.Add(player.Profile.AccountId, true);
        //                itemhandscont.Pickup(true);
        //            }
        //        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
        }
    }
}
