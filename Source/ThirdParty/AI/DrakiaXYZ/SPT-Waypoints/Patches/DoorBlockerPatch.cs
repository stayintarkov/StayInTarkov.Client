using DrakiaXYZ.Waypoints.Components;
using EFT;
using SIT.Tarkov.Core;
using System.Reflection;

namespace DrakiaXYZ.Waypoints.Patches
{
    internal class DoorBlockerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            DoorBlockAdderComponent.Enable();
        }
    }
}
