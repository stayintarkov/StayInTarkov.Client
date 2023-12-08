using DrakiaXYZ.BigBrain.Brains;
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
     * Patch the layer add method (method_0) of AICoreStrategyClass, so we can disable layers, and insert custom layers
     * as higher priority than default layers
     **/
    public class BotBaseBrainAddLayerPatch : ModulePatch
    {
        private static FieldInfo _layerDictionary;
        private static MethodInfo _activateLayerMethod;

        protected override MethodBase GetTargetMethod()
        {
            Type baseBrainType = typeof(BaseBrain);
            Type aiCoreStrategyType = baseBrainType.BaseType;

            _layerDictionary = AccessTools.Field(aiCoreStrategyType, "dictionary_0");
            _activateLayerMethod = AccessTools.GetDeclaredMethods(aiCoreStrategyType).Single(x =>
            {
                var parms = x.GetParameters();
                return (parms.Length == 1 && parms[0].ParameterType == typeof(AICoreLogicLayerClass) && parms[0].Name == "layer");
            });

            return AccessTools.GetDeclaredMethods(aiCoreStrategyType).Single(x =>
            {
                var parms = x.GetParameters();
                return (parms.Length == 3 && parms[0].Name == "index" && parms[1].Name == "layer");
            });
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance, int index, AICoreLogicLayerClass layer, bool activeOnStart, ref bool __result)
        {
            // Make sure we're not excluding this layer from this brain
            BaseBrain botBrain = __instance as BaseBrain;
            foreach (BrainManager.ExcludeLayerInfo excludeInfo in BrainManager.Instance.ExcludeLayers)
            {
                if (layer.Name() == excludeInfo.excludeLayerName && excludeInfo.excludeLayerBrains.Contains(botBrain.ShortName()))
                {
#if DEBUG
                    Logger.LogDebug($"Skipping adding {layer.Name()} to {botBrain.ShortName()} as it was removed");
#endif
                    __result = false;
                    return false;
                }
            }

            Dictionary<int, AICoreLogicLayerClass> layerDictionary = _layerDictionary.GetValue(__instance) as Dictionary<int, AICoreLogicLayerClass>;

            // Make sure the layer index doesn't already exist
            if (layerDictionary.ContainsKey(index))
            {
                Logger.LogError($"Trying add layer with existing index: {index}");
                __result = false;
                return false;
            }

            // Add to the dictionary, and activate if required
            layerDictionary.Add(index, layer);
            if (activeOnStart)
            {
                _activateLayerMethod.Invoke(__instance, new object[] { layer });
            }
            __result = true;

            return false;
        }
    }
}