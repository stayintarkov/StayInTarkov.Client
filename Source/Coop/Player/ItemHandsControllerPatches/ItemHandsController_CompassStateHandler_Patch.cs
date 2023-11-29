using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.ItemHandsControllerPatches
{
    internal class ItemHandsController_CompassStateHandler_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.ItemHandsController);
        public override string MethodName => "CompassStateHandler";
        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player.ItemHandsController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
            {
                return false;
            }

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void Postfix(EFT.Player.ItemHandsController __instance, EFT.Player ____player, bool isActive)
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "c", isActive.ToString() },
                { "m", "CompassStateHandler" }
            };

            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;

            try
            {
                CallLocally.Add(player.ProfileId);
                var itemHandsController = player.HandsController as EFT.Player.ItemHandsController;
                if (itemHandsController != null)
                {
                    if (bool.TryParse(dict["c"].ToString(), out var c))
                    {
                        itemHandsController.CompassStateHandler(c);
                    }
                }
                else
                {
                    Logger.LogInfo("CompassState: itemHandsController was null");
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
