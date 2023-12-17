using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.UsableItemControllerPatches
{
    internal class FireArmController_ToggleAim_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ToggleAim";
        public static List<string> CallLocally = new();

        //[PatchPrefix]
        //public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        //{
        //    var player = ____player;
        //    if (player == null)
        //    {
        //        return false;
        //    }

        //    var result = false;
        //    if (CallLocally.Contains(player.ProfileId))
        //        result = true;

        //    return result;
        //}

        [PatchPostfix]
        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            player.WeaponPacket.ToggleAim = true;
            player.WeaponPacket.AimingIndex = (__instance.IsAiming) ? __instance.Item.AimIndex.Value : -1;
            player.WeaponPacket.ToggleSend();

            //if (CallLocally.Contains(player.ProfileId))
            //{
            //    CallLocally.Remove(player.ProfileId);
            //    return;
            //}

            //Dictionary<string, object> dictionary = new()
            //{
            //    { "m", "ToggleAim" }
            //};

            //AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;

            try
            {
                if (player.HandsController is EFT.Player.FirearmController firearmCont)
                {
                    CallLocally.Add(player.ProfileId);
                    firearmCont.ToggleAim();
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }
    }
}