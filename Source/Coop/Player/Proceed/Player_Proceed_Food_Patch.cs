using EFT.InventoryLogic;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_Proceed_Food_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedFood";

        public static HashSet<string> CallLocally = new();

        public static MethodInfo method1 = null;

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"Player_Proceed_Food_Patch:Type is NULL");

            method1 = ReflectionHelpers.GetAllMethodsForType(t).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "foodDrink");

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method1;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (CallLocally.Contains(__instance.ProfileId) || __instance.IsAI)
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , FoodDrink foodDrink
            , float amount
            , int animationVariant
            , bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId) || __instance.IsAI)
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            // Stop Client Drone sending a Proceed back to the player
            if (__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(foodDrink.CurrentAddress, ref args);

            args.Add("m", "ProceedFood");
            args.Add("amt", amount);
            args.Add("item.id", foodDrink.Id);
            args.Add("item.tpl", foodDrink.TemplateId);
            args.Add("variant", animationVariant);
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

            if (!ItemFinder.TryFindItemOnPlayer(player, dict["item.tpl"].ToString(), dict["item.id"].ToString(), out Item item))
                ItemFinder.TryFindItemInWorld(dict["item.id"].ToString(), out item);

            if (item != null)
            {
                CallLocally.Add(player.ProfileId);
                player.Proceed((FoodDrink)item, float.Parse(dict["amt"].ToString()), (IResult) => { }, 1, true);
            }
        }


    }
}