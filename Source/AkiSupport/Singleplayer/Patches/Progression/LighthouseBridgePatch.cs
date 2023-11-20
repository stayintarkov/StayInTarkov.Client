using Comfort.Common;
using EFT;
using StayInTarkov.AkiSupport.Singleplayer.Components;
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
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.MainPlayer.Location.ToLower() != "lighthouse") return;

            gameWorld.GetOrAddComponent<LighthouseProgressionComponent>();
        }
    }
}