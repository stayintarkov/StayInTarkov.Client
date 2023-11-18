using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_TryProceed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "TryProceed";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"PlayerOnTryProceedPatch:Type is NULL");

            var method = ReflectionHelpers.GetMethodForType(t, MethodName);

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            //var result = false;
            //var player = __instance;

            // This has to happen, otherwise AI freeze on spawn, cos its stupid like that
            //if (player.IsAI)
            //    result = true;

            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            return true;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , Item item
            , bool scheduled)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            // Stop Spawning Client Drone sending a TryProceed back to the player
            if (__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new();
            args.Add("m", "TryProceed");
            args.Add("t", DateTime.Now.Ticks);
            args.Add("item.id", item.Id);
            args.Add("item.tpl", item.TemplateId);
            args.Add("s", scheduled.ToString());
            AkiBackendCommunicationCoop.PostLocalPlayerData(__instance, args);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            Item item;
            if (!ItemFinder.TryFindItemOnPlayer(player, dict["item.tpl"].ToString(), dict["item.id"].ToString(), out item))
                ItemFinder.TryFindItemInWorld(dict["item.id"].ToString(), out item);

            if (item != null)
            {
                //Logger.LogDebug($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Found Item");

                //if (item is Weapon weapon)
                //{
                //    player.Proceed(weapon, (IResult) =>
                //    {
                //        //Logger.LogDebug($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Weapon:Try Proceed Succeeded?:{IResult.Succeed}");
                //    }, true);
                //}
                //else
                //{
                CallLocally.Add(player.Profile.AccountId, true);

                player.TryProceed(item, (IResult) =>
                {
                    //Logger.LogDebug($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Try Proceed Succeeded?:{IResult.Succeed}");
                    if (!IResult.Succeed)
                    {
                    }
                }, bool.Parse(dict["s"].ToString()));

                //}
            }
        }
    }
}