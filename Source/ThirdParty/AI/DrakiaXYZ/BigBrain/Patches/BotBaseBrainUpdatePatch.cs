using DrakiaXYZ.BigBrain.Internal;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using AICoreLogicLayerClass = AICoreLayerClass<BotLogicDecision>;
using AILogicActionResultStruct = AICoreActionResultStruct<BotLogicDecision>;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the base brain Update method so we can trigger Stop/Start methods on custom layers
     **/
    internal class BotBaseBrainUpdatePatch : ModulePatch
    {
        private static MethodInfo _activeLayerGetter;
        private static MethodInfo _activeLayerSetter;
        private static FieldInfo _activeLayerListField;
        private static FieldInfo _onLayerChangedToField;

        protected override MethodBase GetTargetMethod()
        {
            Type botLogicBrainType = typeof(BaseBrain);
            Type botBaseBrainType = botLogicBrainType.BaseType;

            string activeLayerPropertyName = Utils.GetPropertyNameByType(botBaseBrainType, typeof(AICoreLogicLayerClass));
            _activeLayerGetter = AccessTools.PropertyGetter(botBaseBrainType, activeLayerPropertyName);
            _activeLayerSetter = AccessTools.PropertySetter(botBaseBrainType, activeLayerPropertyName);

            _activeLayerListField = Utils.GetFieldByType(botBaseBrainType, typeof(List<AICoreLogicLayerClass>));
            _onLayerChangedToField = Utils.GetFieldByType(botBaseBrainType, typeof(Action<AICoreLogicLayerClass>));

            return AccessTools.Method(botBaseBrainType, "Update");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance, AILogicActionResultStruct prevResult, ref AILogicActionResultStruct? __result)
        {
#if DEBUG
            try
            {
#endif
                if (__instance == null)
                {
                    __result = null;
                    return false;
                }

                // Get values we'll use later
                List<AICoreLogicLayerClass> activeLayerList = _activeLayerListField.GetValue(__instance) as List<AICoreLogicLayerClass>;
                AICoreLogicLayerClass activeLayer = _activeLayerGetter.Invoke(__instance, null) as AICoreLogicLayerClass;

                if (activeLayerList == null)
                {
                    __result = null;
                    return false;
                }

                foreach (AICoreLogicLayerClass layer in activeLayerList)
                {
                    if (layer.ShallUseNow())
                    {
                        if (layer != activeLayer)
                        {
                            // Allow telling custom layers they're stopping
                            if (activeLayer is CustomLayerWrapper customActiveLayer)
                            {
                                customActiveLayer.Stop();
                            }

                            activeLayer = layer;
                            _activeLayerSetter.Invoke(__instance, new object[] { layer });
                            Action<AICoreLogicLayerClass> action = _onLayerChangedToField.GetValue(__instance) as Action<AICoreLogicLayerClass>;
                            if (action != null)
                            {
                                action(activeLayer);
                            }

                            // Allow telling custom layers they're starting
                            if (activeLayer is CustomLayerWrapper customNewLayer)
                            {
                                customNewLayer.Start();
                            }
                        }

                        // Call the active layer's Update() method
                        __result = activeLayer.Update(new AILogicActionResultStruct?(prevResult));
                        return false;
                    }
                }

                // No layers are active, return null
                __result = null;
                return false;

#if DEBUG
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }
#endif
        }
    }
}
