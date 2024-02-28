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
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
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
            int num = Math.Max(0, 200);
            if (num > 0)
            {
                object[] array = new object[1024 * num];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new byte[1024];
                }
                array = null;
                stopwatch.Stop();
            }
        }

        public static void Collect(bool force = false)
        {
            Logger.LogDebug($"Collect({force})");

            Collect(2, GCCollectionMode.Optimized, isBlocking: true, compacting: false, force);
        }

        public static float GetTotalAllocatedMemoryGB()
        {
            return (float)GC.GetTotalMemory(forceFullCollection: true) / 1024f / 1024f;
        }

        public static float PreviousTime { get; set; }

        public static void Collect(int generation, GCCollectionMode gcMode, bool isBlocking, bool compacting, bool force)
        {
            
            if (!force && Time.time < PreviousTime + 600f)
            {
                return;
            }
            if (force)
            {
                float totalAllocatedMemoryGB = GetTotalAllocatedMemoryGB();
                GC.Collect();
                //if (Settings.AggressiveGC)
                {
                    GC.WaitForPendingFinalizers();
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(generation, gcMode, isBlocking, compacting);
                }
            }
            PreviousTime = Time.time;
        }
    }
}
