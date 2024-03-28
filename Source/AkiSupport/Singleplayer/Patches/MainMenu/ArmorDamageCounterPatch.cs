using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.MainMenu
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/MainMenu/ArmorDamageCounterPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class ArmorDamageCounterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
        }

        [PatchPostfix]
        private static void PatchPostfix(DamageInfo damageInfo)
        {
            if (damageInfo.SourceId == null)
            {
                return;
            }

            if (damageInfo.Player == null)
            {
                return;
            }

            if (damageInfo.Player.iPlayer == null)
            {
                return;
            }

            if (!damageInfo.Player.iPlayer.IsYourPlayer)
            {
                return;
            }

            if (damageInfo.Weapon is Weapon)
            {
                if (!Singleton<ItemFactory>.Instance.ItemTemplates.TryGetValue(damageInfo.SourceId, out var template))
                {
                    return;
                }

                if (template is AmmoTemplate bulletTemplate)
                {
                    float absorbedDamage = (float)Math.Round(bulletTemplate.Damage - damageInfo.Damage);
                    damageInfo.Player.iPlayer.Profile.EftStats.SessionCounters.AddFloat(absorbedDamage, ASessionCounterManager.CauseArmorDamage);
                }
            }
        }
    }
}
