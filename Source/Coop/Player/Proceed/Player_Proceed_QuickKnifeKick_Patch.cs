using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    public class Player_Proceed_QuickKnifeKick_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedQuickKnifeKick";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed"
                   && x.GetParameters().Length == 3
                   && x.GetParameters()[0].Name == "knife"
                   && x.GetParameters()[1].Name == "callback"
                   && x.GetParameters()[2].Name == "scheduled"
                   && x.GetParameters()[1].ParameterType == typeof(Callback<IQuickKnifeKickController>));
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            return CallLocally.Contains(__instance.ProfileId);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, KnifeComponent knife, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            //if (knife.Item is Knife knife0)
            {
                PlayerProceedPacket playerProceedPacket = new(__instance.ProfileId, knife.Item.Id, knife.Item.TemplateId, scheduled, "ProceedQuickKnifeKick");
                AkiBackendCommunication.Instance.SendDataToPool(playerProceedPacket.Serialize());
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedPacket playerProceedPacket = new(null, null, null, true, null);
            playerProceedPacket = playerProceedPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, playerProceedPacket))
                return;

            if (ItemFinder.TryFindItem(playerProceedPacket.ItemId, out Item item))
            {
                if (item.TryGetItemComponent(out KnifeComponent knifeComponent))
                {
                    CallLocally.Add(player.ProfileId);
                    player.Proceed(knifeComponent, (Callback<IQuickKnifeKickController>)null, playerProceedPacket.Scheduled);
                }
                else
                {
                    Logger.LogError($"Player_Proceed_QuickKnifeKick_Patch:Replicated. Item {playerProceedPacket.ItemId} doesn't has KnifeComponent!");
                }
            }
            else
            {
                Logger.LogError($"Player_Proceed_QuickKnifeKick_Patch:Replicated. Cannot found item {playerProceedPacket.ItemId}!");
            }
        }
    }
}