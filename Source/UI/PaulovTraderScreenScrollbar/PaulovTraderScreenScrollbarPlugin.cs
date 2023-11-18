using BepInEx;
using DrakiaXYZ.BigBrain.Patches;
using EFT.UI;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SIT.Core.UI.PaulovTraderScreenScrollbar
{
    /// <summary>
    /// Created by: Paulov
    /// </summary>
    [BepInPlugin("com.paulov.ui.PaulovTraderScreenScrollbar", "PaulovTraderScreenScrollbar", "1.0.0.0")]
    [BepInDependency("com.sit.core")]
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
            
            var container = GameObject.Find("Container");

            var traderCards = GameObject.Find("TraderCards");
            var traderCardsRect = traderCards.GetComponent<RectTransform>();
            traderCardsRect.position = new Vector3(0, traderCardsRect.position.y, 0);
            var scrollRect = traderCards.AddComponent<ScrollRect>();
            scrollRect.content = traderCardsRect;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.scrollSensitivity = 20;
            //scrollRect.movementType = ScrollRect.MovementType.Clamped;

            //var contentSizeFitter = traderCards.AddComponent<ContentSizeFitter>();
            //contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        }
    }


}
