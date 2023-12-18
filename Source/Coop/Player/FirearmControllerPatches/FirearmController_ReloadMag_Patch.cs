using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadMag";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, MagazineClass magazine, GridItemAddress gridItemAddress, EFT.Player ____player)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            GridItemAddressDescriptor gridItemAddressDescriptor = (gridItemAddress == null) ? null : OperationToDescriptorHelpers.FromGridItemAddress(gridItemAddress);

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

                player.WeaponPacket.ReloadMag = new()
                {
                    Reload = true,
                    MagId = magazine.Id,
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
