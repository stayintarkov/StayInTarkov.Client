using BepInEx.Logging;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace StayInTarkov.Memory
{
    /// <summary>
    /// Credit: Paulov
    /// Description: A suite of methods to actively clean (free up) memory
    /// </summary>
    public static class GCHelpers
    {
        [DllImport("psapi.dll", EntryPoint = "EmptyWorkingSet")]
        private static extern bool EmptyWorkingSetCall(IntPtr hProcess);

        private static ManualLogSource Logger { get; set; }

        static GCHelpers()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("GCHelpers");
        }

        public static void EmptyWorkingSet()
        {
            EmptyWorkingSetCall(Process.GetCurrentProcess().Handle);
        }

        public static bool Emptying = false;

        public static void EnableGC()
        {
            if (GarbageCollector.GCMode == GarbageCollector.Mode.Disabled)
            {
                Logger.LogDebug($"EnableGC():Enabled GC");
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            }
        }

        public static void DisableGC(bool forceCollect = false)
        {

            if (GarbageCollector.GCMode == GarbageCollector.Mode.Enabled)
            {
                Collect(forceCollect);
                Logger.LogDebug($"DisableGC():Disabled GC");
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            }
        }

        public static void ClearGarbage(bool emptyTheSet = false, bool unloadAssets = true)
        {
            Logger.LogDebug($"ClearGarbage()");

            if (!emptyTheSet)
            {
                EnableGC();
                Collect(force: true);
            }

            if (Emptying)
                return;

            if (unloadAssets)
                Resources.UnloadUnusedAssets();

            if (emptyTheSet)
            {
                Emptying = true;
                RunHeapPreAllocation();
                Collect(force: true);
                EmptyWorkingSet();
            }
            Emptying = false;
            DisableGC();
        }

        public static void RunHeapPreAllocation()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int objectArrayLength = 512;
            int byteArrayLength = 1024;

            object[] array = new object[objectArrayLength];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new byte[byteArrayLength];
            }
            array = null;
            stopwatch.Stop();
            Logger.LogDebug($"Heap pre-allocation took {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = null;

            GC.AddMemoryPressure(objectArrayLength * byteArrayLength);
        }

        public static void Collect(bool force = false)
        {
            Logger.LogDebug($"Collect({force})");

            Collect(4, force ? GCCollectionMode.Forced : GCCollectionMode.Optimized, isBlocking: force, compacting: force, force);
        }

        public static void Collect(int generation, GCCollectionMode gcMode, bool isBlocking, bool compacting, bool force)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(generation, gcMode, isBlocking, compacting);
        }
    }
}
