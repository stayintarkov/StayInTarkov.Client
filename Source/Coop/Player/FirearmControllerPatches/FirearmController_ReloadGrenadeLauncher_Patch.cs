using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadGrenadeLauncher_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadGrenadeLauncher";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static HashSet<string> CallLocally = new();

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, AmmoPack foundItem, EFT.Player ____player)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            var reloadingAmmoIds = foundItem.GetReloadingAmmoIds();

            player.WeaponPacket.ReloadLauncher = new()
            {
                Reload = true,
                AmmoIdsCount = reloadingAmmoIds.Length,
                AmmoIds = reloadingAmmoIds
            };
            player.WeaponPacket.ToggleSend();

            //if (CallLocally.Contains(player.ProfileId))
            //{
            //    CallLocally.Remove(player.ProfileId);
            //    return;
            //}

            //Dictionary<string, object> magAddressDict = new();
            //ItemAddressHelpers.ConvertItemAddressToDescriptor(magazine.CurrentAddress, ref magAddressDict);

            //Dictionary<string, object> gridAddressDict = new();
            //ItemAddressHelpers.ConvertItemAddressToDescriptor(gridItemAddress, ref gridAddressDict);

            //Dictionary<string, object> dictionary = new()
            //{
            //    { "fa.id", __instance.Item.Id },
            //    { "fa.tpl", __instance.Item.TemplateId },
            //    { "mg.id", magazine.Id },
            //    { "mg.tpl", magazine.TemplateId },
            //    { "ma", magAddressDict },
            //    { "ga", gridAddressDict },
            //    { "m", "ReloadMag" }
            //};
            //AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
            //GetLogger().LogDebug("FirearmController_ReloadMag_Patch:PostPatch");

            // ---------------------------------------------------------------------------------------------------------------------
            // Note. If the player is AI or High Ping. Stop the loop caused by the sent packet above
            //if (IsHighPingOrAI(player))
            //{
            //HasProcessed(typeof(FirearmController_ReloadMag_Patch), player, dictionary);
            //}
        }



        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }        
    }
}
