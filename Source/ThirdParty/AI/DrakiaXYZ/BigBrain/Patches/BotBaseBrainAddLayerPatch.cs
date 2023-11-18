using DrakiaXYZ.BigBrain.Brains;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using AICoreLogicLayerClass = AICoreLayerClass<BotLogicDecision>;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the layer add method (method_0) of AICoreStrategyClass, so we can disable layers, and insert custom layers
     * as higher priority than default layers
     **/
    internal class BotBaseBrainAddLayerPatch : ModulePatch
    {
        private static FieldInfo _layerDictionary;
        private static MethodInfo _activateLayerMethod;

        protected override MethodBase GetTargetMethod()
        {
            Type botLogicBrainType = typeof(BaseBrain);
            Type botBaseBrainType = botLogicBrainType.BaseType;

            _layerDictionary = AccessTools.Field(botBaseBrainType, "dictionary_0");
            _activateLayerMethod = AccessTools.Method(botBaseBrainType, "method_4");

            return AccessTools.Method(botBaseBrainType, "method_0");
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
