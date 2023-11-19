using DrakiaXYZ.Waypoints.Components;
using EFT;
using SIT.Tarkov.Core;
using System.Reflection;

namespace DrakiaXYZ.Waypoints.Patches
{
    public class EditorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            //EditorComponent.Enable();
        }
    }
}
