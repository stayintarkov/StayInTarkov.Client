using BepInEx;
using EFT.UI;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace StayInTarkov.UI.PaulovTraderScreenScrollbar
{
    /// <summary>
    /// Created by: Paulov
    /// </summary>
    [BepInPlugin("com.paulov.ui.PaulovTraderScreenScrollbar", "PaulovTraderScreenScrollbar", "1.0.0.0")]
    [BepInDependency("com.stayintarkov")]
    internal class PaulovTraderScreenScrollbarPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogDebug($"Loading: {nameof(PaulovTraderScreenScrollbarPlugin)}");
            try
            {
                new TraderScreensGroupPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogDebug($"Completed: {nameof(PaulovTraderScreenScrollbarPlugin)}");
        }
    }

    class TraderScreensGroupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(TraderScreensGroup), "Awake");
        }

        [PatchPostfix]
        public static void Postfix(TraderScreensGroup __instance)
        {
            var traderCards = GameObject.Find("TraderCards");
            var scrollRect = traderCards.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.scrollSensitivity = 20;
            var container = GameObject.Find("Container");
            var containerRect = container.RectTransform();
            scrollRect.viewport = containerRect;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            var contentSizeFitter = traderCards.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = traderCards.RectTransform();
        }


    }


}
