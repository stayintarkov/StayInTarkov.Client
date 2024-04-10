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

    /// <summary>
    /// Taken from https://raw.githubusercontent.com/BepInEx/BepInEx.Utility/master/BepInEx.ResourceUnloadOptimizations/MemoryInfo.cs
    /// Credits to BepInEx
    /// Provides information about system memory status
    /// </summary>
    internal static class MemoryInfo
        {
            /// <summary>
            /// Can return null if the call fails for whatever reason
            /// </summary>
            public static MEMORYSTATUSEX GetCurrentStatus()
            {
                try
                {
                    var msex = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(msex))
                        return msex;
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }

            /// <summary>
            /// contains information about the current state of both physical and virtual memory, including extended memory
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MEMORYSTATUSEX
            {
                /// <summary>
                /// Size of the structure, in bytes. You must set this member before calling GlobalMemoryStatusEx.
                /// </summary>
                public uint dwLength;

                /// <summary>
                /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use).
                /// </summary>
                public uint dwMemoryLoad;

                /// <summary>
                /// Total size of physical memory, in bytes.
                /// </summary>
                public ulong ullTotalPhys;

                /// <summary>
                /// Size of physical memory available, in bytes.
                /// </summary>
                public ulong ullAvailPhys;

                /// <summary>
                /// Size of the committed memory limit, in bytes. This is physical memory plus the size of the page file, minus a small overhead.
                /// </summary>
                public ulong ullTotalPageFile;

                /// <summary>
                /// Size of available memory to commit, in bytes. The limit is ullTotalPageFile.
                /// </summary>
                public ulong ullAvailPageFile;

                /// <summary>
                /// Total size of the user mode portion of the virtual address space of the calling process, in bytes.
                /// </summary>
                public ulong ullTotalVirtual;

                /// <summary>
                /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes.
                /// </summary>
                public ulong ullAvailVirtual;

                /// <summary>
                /// Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process, in bytes.
                /// </summary>
                public ulong ullAvailExtendedVirtual;

                /// <summary>
                /// Initializes a new instance of the <see cref="T:MEMORYSTATUSEX"/> class.
                /// </summary>
                public MEMORYSTATUSEX()
                {
                    dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                }
            }

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        }

}
