using DrakiaXYZ.BigBrain.Internal;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AICoreLogicLayerClass = AICoreLayerClass<BotLogicDecision>;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the layer activate method (method_4) of AICoreStrategyClass, so we can prioritize custom layers above default layers
     **/
    internal class BotBaseBrainActivateLayerPatch : ModulePatch
    {
        private static FieldInfo _activeLayerListField;

        protected override MethodBase GetTargetMethod()
        {
            Type baaseBrainType = typeof(BaseBrain);
            Type aiCoreStrategyClassType = baaseBrainType.BaseType;

            _activeLayerListField = AccessTools.Field(aiCoreStrategyClassType, "list_0");

            return AccessTools.GetDeclaredMethods(aiCoreStrategyClassType).Single(x =>
            {
                var parms = x.GetParameters();
                return (parms.Length == 1 && parms[0].ParameterType == typeof(AICoreLogicLayerClass) && parms[0].Name == "layer");
            });
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance, AICoreLogicLayerClass layer)
        {
            // For base layers, we can fall back to the original code, as it will add to the end 
            // of the same-priority layers, which will already prioritize custom layers
            if (!(layer is CustomLayerWrapper))
            {
                return true;
            }

            List<AICoreLogicLayerClass> activeLayerList = _activeLayerListField.GetValue(__instance) as List<AICoreLogicLayerClass>;

            layer.Activate();

            // Look for the first layer with an equal or lower priority, and add out layer before it
            for (int i = 0; i < activeLayerList.Count; i++)
            {
                AICoreLogicLayerClass activeLayer = activeLayerList[i];
                if (layer.Priority >= activeLayer.Priority)
                {
                    activeLayerList.Insert(i, layer);
                    return false;
                }
            }
            activeLayerList.Add(layer);

            return false;
        }
    }
}