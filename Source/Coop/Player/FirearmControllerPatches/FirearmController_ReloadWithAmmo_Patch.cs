using EFT.InventoryLogic;
//using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadWithAmmo_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadWithAmmo";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            //Logger.LogInfo("FirearmController_ReloadWithAmmo_Patch:PrePatch");

            //return true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , AmmoPack ammoPack
            , EFT.Player ____player)
        {
            var player = ____player;
            //var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }


            //foreach (var item in ammoPack.GetReloadingAmmoIds())
            //{
            //    Logger.LogInfo($"FirearmController_ReloadWithAmmo_Patch:PostPatch:{item}");
            //}

            Dictionary<string, object> dictionary = new()
            {
                { "ammo", ammoPack.GetReloadingAmmoIds().ToJson() },
                { "m", "ReloadWithAmmo" }
            };
            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            //Logger.LogInfo("FirearmController_ReloadMag_Patch:Replicated");

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    var ammoIds = dict["ammo"].ToString().ParseJsonTo<string[]>();
                    List<BulletClass> list = new();
                    foreach (string ammoId in ammoIds)
                    {
                        var findItem = player.FindItemById(ammoId, false, true);
                        if (findItem.Failed)
                        {
                            Logger.LogInfo("There is no item with id " + ammoId + " in the GameWorld");
                        }
                        Item item = player.FindItemById(ammoId, false, true).Value;
                        BulletClass bulletClass = findItem.Value as BulletClass;
                        if (bulletClass == null)
                            continue;
                        list.Add(bulletClass);
                    }

                    if (!CallLocally.ContainsKey(player.Profile.AccountId))
                        CallLocally.Add(player.Profile.AccountId, true);

                    AmmoPack ammoPack = new(list);
                    //firearmCont.ReloadWithAmmo(ammoPack, (c) => { });
                    firearmCont.StartCoroutine(ReloadWithAmmo(player, firearmCont, ammoPack));
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }

        private IEnumerator ReloadWithAmmo(EFT.Player player
            , EFT.Player.FirearmController firearmCont
            , AmmoPack ammoPack)
        {
            while (!firearmCont.CanStartReload())
            {
                yield return new WaitForSeconds(1);
                yield return new WaitForEndOfFrame();
            }

            GetLogger(typeof(FirearmController_ReloadWithAmmo_Patch)).LogDebug($"{player.ProfileId} Notify to not use ICH Move Patch");
            //ItemControllerHandler_Move_Patch.DisableForPlayer.Add(player.ProfileId);

            firearmCont.ReloadWithAmmo(ammoPack, (c) =>
            {

                if (c.Failed)
                {
                    Logger.LogError($"{player.ProfileId}: Failed to ReloadWithAmmo");
                }

            });
            GetLogger(typeof(FirearmController_ReloadWithAmmo_Patch)).LogDebug($"{player.ProfileId} Notify to use ICH Move Patch");
            //ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(player.ProfileId);

        }
    }
}
