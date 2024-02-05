using Comfort.Common;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Fixes the map sandbox from only spawning 1 bot at start of game as well as fixing no spawns till all bots are dead.
    /// Remove once BSG decides to fix their map
    /// </summary>
    public class FixBrokenSpawnOnSandboxPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                return;
            }

            var playerLocation = gameWorld.MainPlayer.Location;

            if (playerLocation == "Sandbox")
            {
                Object.FindObjectsOfType<BotZone>().ToList().First(x => x.name == "ZoneSandbox").MaxPersonsOnPatrol = 10;
            }
        }
    }
}