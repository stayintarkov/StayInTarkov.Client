using BepInEx.Logging;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov
{
    public abstract class ModulePatch : IDisposable
    {
        private readonly Harmony _harmony;
        //private static Harmony _harmony;
        private readonly List<HarmonyMethod> _prefixList;
        private readonly List<HarmonyMethod> _postfixList;
        private readonly List<HarmonyMethod> _transpilerList;
        private readonly List<HarmonyMethod> _finalizerList;
        private readonly List<HarmonyMethod> _ilmanipulatorList;
        private bool disposedValue;

        private static Dictionary<Type, ManualLogSource> Loggers { get; } = new();

        protected static ManualLogSource Logger
        {
            get
            {
                return GetLogger(typeof(ModulePatch));
            }
        }

        protected static ManualLogSource GetLogger(Type type)
        {
            if (!Loggers.ContainsKey(type))
            {
                Loggers.Add(type, BepInEx.Logging.Logger.CreateLogSource(type.Name));
                Loggers[type].LogEvent += Logger_LogEvent;

            }

            return Loggers[type];
        }
        public string ThisTypeName { get; private set; }

        protected ModulePatch() : this(null)
        {
            ThisTypeName = GetType().FullName;
        }

        private static void Logger_LogEvent(object sender, LogEventArgs e)
        {
            if (e.Level == LogLevel.Error)
            {
                ConsoleScreen.LogError(e.Data.ToString());
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected ModulePatch(string name = null)
        {
            //Tarkov.Core.Plugin.LegalityCheck();
            //if(_harmony == null)
            //_harmony = new Harmony("StayInTarkov");
            _harmony = new Harmony(name ?? GetType().Name);
            _prefixList = GetPatchMethods(typeof(PatchPrefixAttribute));
            _postfixList = GetPatchMethods(typeof(PatchPostfixAttribute));
            _transpilerList = GetPatchMethods(typeof(PatchTranspilerAttribute));
            _finalizerList = GetPatchMethods(typeof(PatchFinalizerAttribute));
            _ilmanipulatorList = GetPatchMethods(typeof(PatchILManipulatorAttribute));

            if (_prefixList.Count == 0
                && _postfixList.Count == 0
                && _transpilerList.Count == 0
                && _finalizerList.Count == 0
                && _ilmanipulatorList.Count == 0)
            {
                throw new Exception($"{_harmony.Id}: At least one of the patch methods must be specified");
            }
        }

        /// <summary>
        /// Get original method
        /// </summary>
        /// <returns>Method</returns>
        protected abstract MethodBase GetTargetMethod();

        /// <summary>
        /// Get HarmonyMethod from string
        /// </summary>
        /// <param name="attributeType">Attribute type</param>
        /// <returns>Method</returns>
        public virtual List<HarmonyMethod> GetPatchMethods(Type attributeType)
        {
            var T = GetType();
            var methods = new List<HarmonyMethod>();

            foreach (var method in T.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (method.GetCustomAttribute(attributeType) != null)
                {
                    methods.Add(new HarmonyMethod(method));
                }
            }

            return methods;
        }

        /// <summary>
        /// Apply patch to target
        /// </summary>
        public virtual void Enable()
        {
            var target = GetTargetMethod();

            if (target == null)
            {
                Logger.LogError($"{GetType().Name}: TargetMethod is null");
                return;
            }

            Logger.LogDebug($"Enabling patch {GetType().Name}");


            try
            {
                if (_harmony.GetPatchedMethods().Any(x => x == target))
                {
                    return;
                }

                foreach (var prefix in _prefixList)
                {
                    _harmony.Patch(target, prefix: prefix);
                }

                foreach (var postfix in _postfixList)
                {
                    _harmony.Patch(target, postfix: postfix);
                }

                foreach (var transpiler in _transpilerList)
                {
                    _harmony.Patch(target, transpiler: transpiler);
                }

                foreach (var finalizer in _finalizerList)
                {
                    _harmony.Patch(target, finalizer: finalizer);
                }

                foreach (var ilmanipulator in _ilmanipulatorList)
                {
                    _harmony.Patch(target, ilmanipulator: ilmanipulator);
                }

                //Logger.LogDebug($"Enabled patch {GetType().Name}");
                Logger.LogInfo($"Enabled patch {GetType().Name}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{GetType().Name}:", ex);
            }
        }

        /// <summary>
        /// Remove applied patch from target
        /// </summary>
        public virtual void Disable()
        {
            var target = GetTargetMethod();

            if (target == null)
            {
                Logger.LogError($"{GetType().Name}: TargetMethod is null");
            }

            try
            {
                _harmony.Unpatch(target, HarmonyPatchType.All, _harmony.Id);
                //Logger.LogDebug($"Disabled patch {GetType().Name}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{GetType().Name}:", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
