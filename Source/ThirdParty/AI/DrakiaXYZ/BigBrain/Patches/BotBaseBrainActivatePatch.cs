using DrakiaXYZ.BigBrain.Brains;
using DrakiaXYZ.BigBrain.Internal;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Linq;
using System.Reflection;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the base brain activate method so we can inject our custom brain layers
     **/
    public class BotBaseBrainActivatePatch : ModulePatch
    {
        private static FieldInfo _botOwnerField;
        private static MethodInfo _addLayerMethod;
        protected override MethodBase GetTargetMethod()
        {
            Type baseBrainType = typeof(BaseBrain);
            Type aiCoreStrategyType = baseBrainType.BaseType;

            _botOwnerField = AccessTools.GetDeclaredFields(baseBrainType).Single(x => x.FieldType == typeof(BotOwner));
            _addLayerMethod = AccessTools.GetDeclaredMethods(aiCoreStrategyType).Single(x =>
            {
                var parms = x.GetParameters();
                return (parms.Length == 3 && parms[0].Name == "index" && parms[1].Name == "layer");
            });

            return AccessTools.Method(aiCoreStrategyType, "Activate");
        }

        [PatchPrefix]
        public static void PatchPrefix(object __instance)
        {
            try
            {
                BaseBrain botBrain = __instance as BaseBrain;
                BotOwner botOwner = (BotOwner)_botOwnerField.GetValue(botBrain);

                foreach (BrainManager.LayerInfo layerInfo in BrainManager.Instance.CustomLayers.Values)
                {
                    if (layerInfo.customLayerBrains.Contains(botBrain.ShortName()))
                    {
                        CustomLayerWrapper customLayerWrapper = new CustomLayerWrapper(layerInfo.customLayerType, botOwner, layerInfo.customLayerPriority);
#if DEBUG
                        Logger.LogDebug($"  Injecting {customLayerWrapper.Name()}({layerInfo.customLayerId}) with priority {layerInfo.customLayerPriority}");
#endif
                        _addLayerMethod.Invoke(botBrain, new object[] { layerInfo.customLayerId, customLayerWrapper, true });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}