using BepInEx.Logging;
using EFT;
using Newtonsoft.Json;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StayInTarkov.AkiSupport.Custom.CustomAI
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT/Modules/src/branch/master/project/SPT.Custom/CustomAI/AIBrainSpawnWeightAdjustment.cs
    /// </summary>
    public class AIBrainSpawnWeightAdjustment
    {
        private static AIBrains aiBrainsCache = null;
        private static DateTime aiBrainCacheDate = new DateTime();
        private static readonly Random random = new Random();
        private readonly ManualLogSource logger;

        public AIBrainSpawnWeightAdjustment(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public WildSpawnType GetRandomisedPlayerScavType(BotOwner botOwner, string currentMapName)
        {
            // Get map brain weights from server and cache
            if (aiBrainsCache == null || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!aiBrainsCache.playerScav.TryGetValue(currentMapName.ToLower(), out _))
                {
                    throw new Exception($"Bots were refreshed from the server but the assault cache still doesnt contain data");
                }
            }

            // Choose random weighted brain
            var randomType = WeightedRandom(aiBrainsCache.playerScav[currentMapName.ToLower()].Keys.ToArray(), aiBrainsCache.playerScav[currentMapName.ToLower()].Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                logger.LogWarning($"Updated player scav bot to use: {newAiType} brain");
                return newAiType;
            }
            else
            {
                logger.LogWarning($"Updated player scav bot {botOwner.Profile.Info.Nickname}: {botOwner.Profile.Info.Settings.Role} to use: {newAiType} brain");

                return newAiType;
            }
        }

        public WildSpawnType GetAssaultScavWildSpawnType(BotOwner botOwner, string currentMapName)
        {
            // Get map brain weights from server and cache
            if (aiBrainsCache == null || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!aiBrainsCache.assault.TryGetValue(currentMapName.ToLower(), out _))
                {
                    throw new Exception($"Bots were refreshed from the server but the assault cache still doesnt contain data");
                }
            }

            // Choose random weighted brain
            var randomType = WeightedRandom(aiBrainsCache.assault[currentMapName.ToLower()].Keys.ToArray(), aiBrainsCache.assault[currentMapName.ToLower()].Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                logger.LogWarning($"Updated assault bot to use: {newAiType} brain");
                return newAiType;
            }
            else
            {
                logger.LogWarning($"Updated assault bot {botOwner.Profile.Info.Nickname}: {botOwner.Profile.Info.Settings.Role} to use: {newAiType} brain");

                return newAiType;
            }
        }

        public WildSpawnType GetPmcWildSpawnType(BotOwner botOwner_0, WildSpawnType pmcType, string currentMapName)
        {
            if (aiBrainsCache == null || !aiBrainsCache.pmc.TryGetValue(pmcType, out var botSettings) || CacheIsStale())
            {
                ResetCacheDate();
                HydrateCacheWithServerData();

                if (!aiBrainsCache.pmc.TryGetValue(pmcType, out botSettings))
                {
                    throw new Exception($"Bots were refreshed from the server but the cache still doesnt contain an appropriate bot for type {botOwner_0.Profile.Info.Settings.Role}");
                }
            }

            var mapSettings = botSettings[currentMapName.ToLower()];
            var randomType = WeightedRandom(mapSettings.Keys.ToArray(), mapSettings.Values.ToArray());
            if (Enum.TryParse(randomType, out WildSpawnType newAiType))
            {
                logger.LogWarning($"Updated spt bot {botOwner_0.Profile.Info.Nickname}: {botOwner_0.Profile.Info.Settings.Role} to use: {newAiType} brain");

                return newAiType;
            }
            else
            {
                logger.LogError($"Couldnt not update spt bot {botOwner_0.Profile.Info.Nickname} to random type {randomType}, does not exist for WildSpawnType enum, defaulting to 'assault'");

                return WildSpawnType.assault;
            }
        }

        private void HydrateCacheWithServerData()
        {
            // Get weightings for PMCs from server and store in dict
            var result = AkiBackendCommunication.Instance.GetJsonBLOCKING($"/singleplayer/settings/bot/getBotBehaviours/");
            aiBrainsCache = JsonConvert.DeserializeObject<AIBrains>(result);
            logger.LogWarning($"Cached ai brain weights in client");
        }

        private void ResetCacheDate()
        {
            aiBrainCacheDate = DateTime.Now;
            aiBrainsCache?.pmc?.Clear();
            aiBrainsCache?.assault?.Clear();
            aiBrainsCache?.playerScav?.Clear();
        }

        private static bool CacheIsStale()
        {
            TimeSpan cacheAge = DateTime.Now - aiBrainCacheDate;

            return cacheAge.Minutes > 15;
        }

        public class AIBrains
        {
            public Dictionary<WildSpawnType, Dictionary<string, Dictionary<string, int>>> pmc { get; set; }
            public Dictionary<string, Dictionary<string, int>> assault { get; set; }
            public Dictionary<string, Dictionary<string, int>> playerScav { get; set; }
        }

        /// <summary>
        /// Choose a value from a choice of values with weightings
        /// </summary>
        /// <param name="botTypes"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        private string WeightedRandom(string[] botTypes, int[] weights)
        {
            var cumulativeWeights = new int[botTypes.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeWeights[i] = weights[i] + (i == 0 ? 0 : cumulativeWeights[i - 1]);
            }

            var maxCumulativeWeight = cumulativeWeights[cumulativeWeights.Length - 1];
            var randomNumber = maxCumulativeWeight * random.NextDouble();

            for (var itemIndex = 0; itemIndex < botTypes.Length; itemIndex++)
            {
                if (cumulativeWeights[itemIndex] >= randomNumber)
                {
                    return botTypes[itemIndex];
                }
            }

            logger.LogError("failed to get random bot weighting, returned assault");

            return "assault";
        }
    }

}
