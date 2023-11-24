﻿using EFT.UI;
using System.Reflection;

namespace StayInTarkov.Coop.Matchmaker
{
    public class EnvironmentUIRootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EnvironmentUIRoot).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EnvironmentUIRoot __instance)
        {
            MatchmakerAcceptPatches.EnvironmentUIRoot = __instance.gameObject;
        }
    }
}
