using Comfort.Common;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_Proceed_QuickKnifeKick_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedQuickKnifeKick";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(typeof(EFT.Player)).FirstOrDefault(x => x.Name == "Proceed"
                   && x.GetParameters().Length == 3
                   && x.GetParameters()[0].Name == "knife"
                   && x.GetParameters()[1].Name == "callback"
                   && x.GetParameters()[2].Name == "scheduled"
                   && x.GetParameters()[1].ParameterType == typeof(Callback<IHandsController7>));
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            if (CallLocally.Contains(__instance.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, KnifeComponent knife, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            // Stop Client Drone sending a Proceed back to the player
            if (__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc) && prc.IsClientDrone)
                return;

            if (knife.Item is Knife0 knife0)
            {
                Dictionary<string, object> args = new();
                ItemAddressHelpers.ConvertItemAddressToDescriptor(knife0.CurrentAddress, ref args);

                args.Add("m", "ProceedQuickKnifeKick");
                args.Add("item.id", knife0.Id);
                args.Add("s", scheduled.ToString());
                AkiBackendCommunicationCoop.PostLocalPlayerData(__instance, args);
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            string itemId = dict["item.id"].ToString();

            if (ItemFinder.TryFindItem(itemId, out Item item))
            {
                if (item.TryGetItemComponent(out KnifeComponent knifeComponent))
                {
                    CallLocally.Add(player.ProfileId);
                    Callback<IHandsController7> callback = null;
                    player.Proceed(knifeComponent, callback, bool.Parse(dict["s"].ToString()));
                }
                else
                {
                    Logger.LogError($"Player_Proceed_QuickKnifeKick_Patch:Replicated. Item {itemId} is not a KnifeComponent!");
                }
            }
            else
            {
                Logger.LogError($"Player_Proceed_QuickKnifeKick_Patch:Replicated. Cannot found item {itemId}!");
            }
        }
    }
}
