using Comfort.Common;
using EFT;
using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Components;
using StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Progression
{
    /// <summary>
    /// Credit SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/Progression/LighthouseBridgePatch.cs
    /// </summary>
    public class LighthouseBridgePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null)
            {
                return;
            }

            if (gameWorld.MainPlayer.Location.ToLower() != "lighthouse" || gameWorld.MainPlayer.Side == EPlayerSide.Savage)
            {
                return;
            }

            gameWorld.GetOrAddComponent<LighthouseProgressionComponent>();
        }
    }
}