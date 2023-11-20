using EFT.UI;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace StayInTarkov.Coop.FreeCamera
{
    internal class FadeBlackScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(PreloaderUI), "FadeBlackScreen");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    internal class StartBlackScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(PreloaderUI), "StartBlackScreenShow");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }

        [PatchPostfix]
        public static void Postfix(Action callback)
        {
            if (callback != null)
            {
                callback();
            }
        }
    }

    internal class SetBlackImageAlphaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(PreloaderUI), "SetBlackImageAlpha");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }

        [PatchPostfix]
        public static void Postfix(
            float alpha,
            Image ____overlapBlackImage
            )
        {
            ____overlapBlackImage.gameObject.SetActive(value: true);
            ____overlapBlackImage.color = new Color(0f, 0f, 0f, 0f);

        }
    }
}
