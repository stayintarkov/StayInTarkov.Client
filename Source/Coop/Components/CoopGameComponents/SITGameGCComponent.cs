using Comfort.Common;
using EFT;
using StayInTarkov.Configuration;
using StayInTarkov.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    public sealed class SITGameGCComponent : MonoBehaviour
    {
        private DateTime LastTimeRun { get; set; } = DateTime.MinValue;
        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        private int LastNumberOfPlayers { get; set; }

        private int NumberOfAlivePlayers => Singleton<GameWorld>.Instance.AllAlivePlayersList.Count;

        #region Unity methods

        void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITGameGCComponent));
            Logger.LogDebug($"{nameof(SITGameGCComponent)}:{nameof(Awake)}");
        }

        void Start()
        {
            Logger.LogDebug($"{nameof(SITGameGCComponent)}:{nameof(Start)}");
            GarbageCollect();
        }

        void Update()
        {
            if((DateTime.Now - LastTimeRun).TotalSeconds > PluginConfigSettings.Instance.AdvancedSettings.SETTING_SITGCMemoryCheckTime)
            {
                LastTimeRun = DateTime.Now;
                GarbageCollectSIT();
            }

            if (NumberOfAlivePlayers != LastNumberOfPlayers)
            {
                LastNumberOfPlayers = NumberOfAlivePlayers;
                BSGMemoryGC.Collect(force: false);
            }
        }

        #endregion

        private void GarbageCollect()
        {
            Logger.LogDebug($"{nameof(GarbageCollect)}");
            BSGMemoryGC.RunHeapPreAllocation();
            BSGMemoryGC.Collect(force: true);
            BSGMemoryGC.EmptyWorkingSet();
            BSGMemoryGC.GCEnabled = true;
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Runs the Garbage Collection
        /// </summary>
        /// <returns></returns>
        private void GarbageCollectSIT()
        {
            if (!PluginConfigSettings.Instance.AdvancedSettings.SETTING_EnableSITGC)
                return;

            var nearestEnemyDist = float.MaxValue;
            foreach (var p in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if (p.ProfileId == Singleton<GameWorld>.Instance.MainPlayer.ProfileId)
                    continue;

                var dist = Vector3.Distance(p.Transform.position, Singleton<GameWorld>.Instance.MainPlayer.Transform.position);
                if (dist < nearestEnemyDist)
                    nearestEnemyDist = dist;
            }

            if (nearestEnemyDist > 10)
            {
                var mem = MemoryInfo.GetCurrentStatus();
                if (mem == null)
                {
                    return;
                }

                var memPercentInUse = mem.dwMemoryLoad;
                Logger.LogDebug($"Total memory used: {mem.dwMemoryLoad}%");
                if (memPercentInUse > PluginConfigSettings.Instance.AdvancedSettings.SETTING_SITGCMemoryThreshold)
                    GarbageCollect();

            }
        }

    }
}
