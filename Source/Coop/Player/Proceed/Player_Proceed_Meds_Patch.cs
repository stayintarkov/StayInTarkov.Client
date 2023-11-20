using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player
{
    internal class Player_Proceed_Meds_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedMeds";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"Player_Proceed_Meds_Patch:Type is NULL");

            var method = ReflectionHelpers.GetAllMethodsForType(t).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "meds");

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (CallLocally.Contains(__instance.ProfileId))
                return true;

            if (IsHighPingOrAI(__instance))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , Meds0 meds, EBodyPart bodyPart, int animationVariant, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
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

            //Dictionary<string, object> args = new();
            //ItemAddressHelpers.ConvertItemAddressToDescriptor(meds.CurrentAddress, ref args);

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            //args.Add("m", "ProceedMeds");
            //args.Add("t", DateTime.Now.Ticks);
            //args.Add("bodyPart", bodyPart.ToString());
            //args.Add("item.id", meds.Id);
            //args.Add("item.tpl", meds.TemplateId);
            //args.Add("variant", animationVariant);
            //args.Add("s", scheduled.ToString());
            //AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(__instance, args);
            var packet = new ProceedMedsPacket(__instance.ProfileId, meds.Id, meds.TemplateId, bodyPart.ToString(), animationVariant);
            AkiBackendCommunication.Instance.SendDataToPool(packet.Serialize());

            if (IsHighPingOrAI(__instance))
            {
                HasProcessed(typeof(Player_Proceed_Meds_Patch), __instance, packet);
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            ProceedMedsPacket proceedMedsPacket = new(player.ProfileId, null, null, null, 0);
            proceedMedsPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, proceedMedsPacket))
                return;

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            Item item;
            if (!ItemFinder.TryFindItemOnPlayer(player, proceedMedsPacket.TemplateId, proceedMedsPacket.ItemId, out item))
                ItemFinder.TryFindItemInWorld(proceedMedsPacket.ItemId, out item);

            if (item == null)
                return;

            var meds = item as Meds0;
            if (meds == null)
                return;

            CallLocally.Add(player.ProfileId);
            player.Proceed(meds, (EBodyPart)Enum.Parse(typeof(EBodyPart), proceedMedsPacket.BodyPart, true), (IResult) => { }, proceedMedsPacket.Variant, true);
        }
    }
}