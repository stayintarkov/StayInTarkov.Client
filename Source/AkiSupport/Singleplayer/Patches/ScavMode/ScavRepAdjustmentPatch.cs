using Comfort.Common;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Patches/ScavMode/ScavRepAdjustmentPatch.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch and Class Fix
    /// </summary>
    public class ScavRepAdjustmentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AStatisticsManagerForPlayer), nameof(AStatisticsManagerForPlayer.OnEnemyKill));
        }

        [PatchPrefix]
        private static void PatchPrefix(string playerProfileId, out Tuple<Player, bool> __state)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            __state = new Tuple<Player, bool>(null, false);

            if (player.Profile.Side != EPlayerSide.Savage)
            {
                return;
            }

            if (Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(playerProfileId) is Player killedPlayer)
            {
                __state = new Tuple<Player, bool>(killedPlayer, killedPlayer.AIData.IsAI);
                killedPlayer.AIData.IsAI = false;
                player.Loyalty.method_1(killedPlayer);
            }
        }
        [PatchPostfix]
        private static void PatchPostfix(Tuple<Player, bool> __state)
        {
            if(__state.Item1 != null)
            {
                __state.Item1.AIData.IsAI = __state.Item2;
            }
        }
    }
}
