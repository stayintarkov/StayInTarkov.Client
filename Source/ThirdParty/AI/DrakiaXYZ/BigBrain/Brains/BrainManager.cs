using DrakiaXYZ.BigBrain.Internal;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;

using AICoreLogicAgentClass = AICoreAgentClass<BotLogicDecision>;
using AICoreLogicLayerClass = AICoreLayerClass<BotLogicDecision>;

namespace DrakiaXYZ.BigBrain.Brains
{
    public class BrainManager
    {
        public const int START_LAYER_ID = 9000;
        public const int START_LOGIC_ID = 9000;

        private static BrainManager _instance;
        internal static BrainManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BrainManager();
                }

                return _instance;
            }
        }

        private static int _currentLayerId = START_LAYER_ID;

        internal Dictionary<int, LayerInfo> CustomLayers = new();
        internal Dictionary<Type, int> CustomLogics = new();
        internal List<Type> CustomLogicList = new();
        internal List<ExcludeLayerInfo> ExcludeLayers = new();

        private static FieldInfo _strategyField = Utils.GetFieldByType(typeof(AICoreLogicAgentClass), typeof(AICoreStrategyClass<>));

        // Hide the constructor so we can have this as a guaranteed singleton
        private BrainManager() { }

        internal class LayerInfo
        {
            public Type customLayerType;
            public List<string> customLayerBrains;
            public int customLayerPriority;
            public int customLayerId;

            public LayerInfo(Type layerType, List<string> layerBrains, int layerPriority, int layerId)
            {
                customLayerType = layerType;
                customLayerBrains = layerBrains;
                customLayerPriority = layerPriority;
                customLayerId = layerId;
            }
        }

        internal class ExcludeLayerInfo
        {
            public List<string> excludeLayerBrains;
            public string excludeLayerName;

            public ExcludeLayerInfo(string layerName, List<string> brains)
            {
                excludeLayerBrains = brains;
                excludeLayerName = layerName;
            }
        }

        public static int AddCustomLayer(Type customLayerType, List<string> brainNames, int customLayerPriority)
        {
            if (!typeof(CustomLayer).IsAssignableFrom(customLayerType))
            {
                throw new ArgumentException($"Custom layer type {customLayerType.FullName} must inherit CustomLayer");
            }

            if (brainNames.Count == 0)
            {
                throw new ArgumentException($"Custom layer type {customLayerType.FullName} must specify at least 1 brain to be added to");
            }

            int customLayerId = _currentLayerId++;
            Instance.CustomLayers.Add(customLayerId, new LayerInfo(customLayerType, brainNames, customLayerPriority, customLayerId));
            return customLayerId;
        }

        public static void AddCustomLayers(List<Type> customLayerTypes, List<string> brainNames, int customLayerPriority)
        {
            customLayerTypes.ForEach(customLayerType => AddCustomLayer(customLayerType, brainNames, customLayerPriority));
        }

        public static void RemoveLayer(string layerName, List<string> brainNames)
        {
            Instance.ExcludeLayers.Add(new ExcludeLayerInfo(layerName, brainNames));
        }

        public static void RemoveLayers(List<string> layerNames, List<string> brainNames)
        {
            layerNames.ForEach(layerName => RemoveLayer(layerName, brainNames));
        }

        public static bool IsCustomLayerActive(BotOwner botOwner)
        {
            object activeLayer = GetActiveLayer(botOwner);
            if (activeLayer is CustomLayer)
            {
                return true;
            }

            return false;
        }

        public static string GetActiveLayerName(BotOwner botOwner)
        {
            return botOwner.Brain.ActiveLayerName();
        }

        /**
         * Return the currently active base layer, which will extend "AICoreLayerClass", or the active
         * CustomLayer if a custom layer is enabled
         **/
        public static object GetActiveLayer(BotOwner botOwner)
        {
            if (botOwner?.Brain?.Agent == null)
            {
                return null;
            }

            BaseBrain botBrainStrategy = _strategyField.GetValue(botOwner.Brain.Agent) as BaseBrain;
            if (botBrainStrategy == null)
            {
                return null;
            }

            AICoreLogicLayerClass activeLayer = botBrainStrategy.CurLayerInfo;
            if (activeLayer is CustomLayerWrapper customLayerWrapper)
            {
                return customLayerWrapper.CustomLayer();
            }

            return activeLayer;
        }

        /**
         * Return the current active logic instance, which will extend "BaseNodeClass", or the active
         * CustomLogic if a custom logic is enabled
         * Note: This is mostly here for BotDebug, please don't use this in plugins
         **/
        public static object GetActiveLogic(BotOwner botOwner)
        {
            if (botOwner == null)
            {
                return null;
            }

            BaseNodeClass activeLogic = CustomLayerWrapper.GetLogicInstance(botOwner);
            if (activeLogic is CustomLogicWrapper customLogicWrapper)
            {
                return customLogicWrapper.CustomLogic();
            }

            return activeLogic;
        }
    }
}
