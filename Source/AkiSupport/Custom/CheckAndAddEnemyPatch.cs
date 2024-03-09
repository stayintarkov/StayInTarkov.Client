using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/CheckAndAddEnemyPatch.cs
    /// </summary>
    public class CheckAndAddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.CheckAndAddEnemy));
        }

        /// <summary>
        /// CheckAndAddEnemy()
        /// Goal: This patch lets bosses shoot back once a PMC has shot them
        /// Removes the !player.AIData.IsAI  check
		/// BSG changed the way CheckAndAddEnemy Works in 14.0 Returns a bool now
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer player, ref bool __result)
        {
            // Set result to not include !player.AIData.IsAI checks
            __result = player.HealthController.IsAlive && !__instance.Enemies.ContainsKey(player) && __instance.AddEnemy(player, EBotEnemyCause.checkAddTODO);
            return false; // Skip Original
        }
    }
}
