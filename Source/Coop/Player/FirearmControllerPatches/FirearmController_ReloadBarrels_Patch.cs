using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadBarrels_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadBarrels";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, AmmoPack ammoPack, GridItemAddress placeToPutContainedAmmoMagazine, EFT.Player ____player)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            GridItemAddressDescriptor gridItemAddressDescriptor = (placeToPutContainedAmmoMagazine == null) ? null : OperationToDescriptorHelpers.FromGridItemAddress(placeToPutContainedAmmoMagazine);

            var ammoIds = ammoPack.GetReloadingAmmoIds();

            using (MemoryStream memoryStream = new())
            {
                using BinaryWriter binaryWriter = new(memoryStream);
                byte[] locationDescription;
                if (gridItemAddressDescriptor != null)
                {
                    binaryWriter.Write(gridItemAddressDescriptor);
                    locationDescription = memoryStream.ToArray();
                }
                else
                {
                    locationDescription = new byte[0];
                }

                EFT.UI.ConsoleScreen.Log("Firing away ReloadMag packet!");

                player.WeaponPacket.ReloadBarrels = new()
                {
                    Reload = true,
                    AmmoIdsCount = ammoIds.Length,
                    AmmoIds = ammoIds,
                    LocationLength = locationDescription.Length,
                    LocationDescription = locationDescription,
                };
                player.WeaponPacket.ToggleSend();
            }

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

            //ItemControllerHandler_Move_Patch.DisableForPlayer.RemoveWhere(x => x == player.ProfileId);
        }



        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
