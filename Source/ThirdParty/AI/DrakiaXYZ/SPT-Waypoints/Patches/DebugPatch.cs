using Comfort.Common;
using DrakiaXYZ.Waypoints.Components;
using DrakiaXYZ.Waypoints.Helpers;
using EFT;
using StayInTarkov;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace DrakiaXYZ.Waypoints.Patches
{
    public class DebugPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            //BotZoneDebugComponent.Enable();
            NavMeshDebugComponent.Enable();
        }
    }
}
