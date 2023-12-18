using Newtonsoft.Json;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_QuickReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "QuickReloadMag";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player, MagazineClass magazine)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer && (!MatchmakerAcceptPatches.IsServer && !player.IsAI))
                return;

            player.WeaponPacket.QuickReloadMag = new()
            {
                Reload = true,
                MagId = magazine.Id
            };
            player.WeaponPacket.ToggleSend();


            //Dictionary<string, object> magAddressDict = new();
            //ItemAddressHelpers.ConvertItemAddressToDescriptor(magazine.CurrentAddress, ref magAddressDict);

            //Dictionary<string, object> dictionary = new()
            //{
            //    { "fa.id", __instance.Item.Id },
            //    { "fa.tpl", __instance.Item.TemplateId },
            //    { "mg.id", magazine.Id },
            //    { "mg.tpl", magazine.TemplateId },
            //    { "ma", magAddressDict },
            //    { "m", "ReloadMag" }
            //};
            //AkiBackendCommunicationCoop.PostLocalPlayerData(____player, dictionary);

            //// Quick Reload is not Round Tripped. 
            //HasProcessed(typeof(FirearmController_QuickReloadMag_Patch), ____player, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}