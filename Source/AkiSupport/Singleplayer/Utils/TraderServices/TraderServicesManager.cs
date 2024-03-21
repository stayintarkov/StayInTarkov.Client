using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BackendConfigSettingsClass;
using TraderServiceClass = TraderServiceAvailabilityData;
using QuestDictClass = GClass2133<string>;
using StandingListClass = GClass2135<float>;
using StayInTarkov.Networking;
using EFT.UI;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Utils/TraderServices/TraderServicesManager.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    public class TraderServicesManager
    {
        /// <summary>
        /// Subscribe to this event to trigger trader service logic.
        /// </summary>
        public event Action<ETraderServiceType, string> OnTraderServicePurchased;

        private static TraderServicesManager _instance;

        public static TraderServicesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TraderServicesManager();
                }

                return _instance;
            }
        }

        private Dictionary<ETraderServiceType, Dictionary<string, bool>> _servicePurchased { get; set; }
        private HashSet<string> _cachedTraders = new HashSet<string>();
        private FieldInfo _playerQuestControllerField;

        public TraderServicesManager()
        {
            _servicePurchased = new Dictionary<ETraderServiceType, Dictionary<string, bool>>();
            _playerQuestControllerField = AccessTools.Field(typeof(Player), "_questController");
        }

        public void Clear()
        {
            _servicePurchased.Clear();
            _cachedTraders.Clear();
        }

        public void GetTraderServicesDataFromServer(string traderId)
        {
            Dictionary<ETraderServiceType, ServiceData> servicesData = Singleton<BackendConfigSettingsClass>.Instance.ServicesData;
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld?.MainPlayer;

            if (gameWorld == null || player == null)
            {
                Debug.LogError("GetTraderServicesDataFromServer - Error fetching game objects");
                return;
            }

            if (!player.Profile.TradersInfo.TryGetValue(traderId, out Profile.TraderInfo traderInfo))
            {
                Debug.LogError("GetTraderServicesDataFromServer - Error fetching profile trader info");
                return;
            }

            // Only request data from the server if it's not already cached
            if (!_cachedTraders.Contains(traderId))
            {
                var json = AkiBackendCommunication.Instance.PostJson($"/singleplayer/traderServices/getTraderServices/{traderId}", new
                {
                    SessionId = player.Profile.ProfileId
                }.ToJson());
                //var json = AkiBackendCommunication.Instance.GetJson($"/singleplayer/traderServices/getTraderServices/{traderId}");
                var traderServiceModels = JsonConvert.DeserializeObject<List<TraderServiceModel>>(json);

                foreach (var traderServiceModel in traderServiceModels)
                {
                    ETraderServiceType serviceType = traderServiceModel.ServiceType;
                    ServiceData serviceData;

                    // Only populate trader services that don't exist yet
                    if (!servicesData.ContainsKey(traderServiceModel.ServiceType))
                    {
                        TraderServiceClass traderService = new TraderServiceClass
                        {
                            TraderId = traderId,
                            ServiceType = serviceType,
                            UniqueItems = traderServiceModel.ItemsToReceive ?? new MongoID[0],
                            ItemsToPay = traderServiceModel.ItemsToPay ?? new Dictionary<MongoID, int>(),

                            // SubServices seem to be populated dynamically in the client (For BTR taxi atleast), so we can just ignore it
                            // NOTE: For future reference, this is a dict of `point id` to `price` for the BTR taxi
                            SubServices = new Dictionary<string, int>()
                        };

                        // Convert our format to the backend settings format
                        serviceData = new ServiceData(traderService);

                        // Populate requirements if provided
                        if (traderServiceModel.Requirements != null)
                        {
                            if (traderServiceModel.Requirements.Standings != null)
                            {
                                serviceData.TraderServiceRequirements.Standings = new StandingListClass();
                                serviceData.TraderServiceRequirements.Standings.AddRange(traderServiceModel.Requirements.Standings);

                                // BSG has a bug in their code, we _need_ to initialize this if Standings isn't null
                                serviceData.TraderServiceRequirements.CompletedQuests = new QuestDictClass();
                            }

                            if (traderServiceModel.Requirements.CompletedQuests != null)
                            {
                                serviceData.TraderServiceRequirements.CompletedQuests = new QuestDictClass();
                                serviceData.TraderServiceRequirements.CompletedQuests.Concat(traderServiceModel.Requirements.CompletedQuests);
                            }
                        }

                        servicesData[serviceData.ServiceType] = serviceData;
                    }
                }

                _cachedTraders.Add(traderId);
            }

            // Update service availability
            foreach (var servicesDataPair in servicesData)
            {
                // Only update this trader's services
                if (servicesDataPair.Value.TraderId != traderId)
                {
                    continue;
                }

                var IsServiceAvailable = this.IsServiceAvailable(player, servicesDataPair.Value.TraderServiceRequirements);

                // Check whether we've purchased this service yet
                var traderService = servicesDataPair.Key;
                var WasPurchasedInThisRaid = IsServicePurchased(traderService, traderId);
                traderInfo.SetServiceAvailability(traderService, IsServiceAvailable, WasPurchasedInThisRaid);
            }
        }

        private bool IsServiceAvailable(Player player, ServiceRequirements requirements)
        {
            // Handle standing requirements
            if (requirements.Standings != null)
            {
                foreach (var entry in requirements.Standings)
                {
                    if (!player.Profile.TradersInfo.ContainsKey(entry.Key) ||
                        player.Profile.TradersInfo[entry.Key].Standing < entry.Value)
                    {
                        return false;
                    }
                }
            }

            // Handle quest requirements

            ConsoleScreen.Log($"GetTraderServicesDataFromServer - servicesDataPair - requirements.CompletedQuests {requirements.CompletedQuests}");
            if (requirements.CompletedQuests != null)
            {
                AbstractQuestControllerClass questController = _playerQuestControllerField.GetValue(player) as AbstractQuestControllerClass;
                foreach (string questId in requirements.CompletedQuests)
                {
                    var conditional = questController.Quests.GetConditional(questId);
                    if (conditional == null || conditional.QuestStatus != EQuestStatus.Success)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void AfterPurchaseTraderService(ETraderServiceType serviceType, AbstractQuestControllerClass questController, string subServiceId = null)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld?.MainPlayer;

            if (gameWorld == null || player == null)
            {
                Debug.LogError("TryPurchaseTraderService - Error fetching game objects");
                return;
            }

            // Service doesn't exist
            if (!Singleton<BackendConfigSettingsClass>.Instance.ServicesData.TryGetValue(serviceType, out var serviceData))
            {
                return;
            }

            SetServicePurchased(serviceType, subServiceId, serviceData.TraderId);
        }

        public void SetServicePurchased(ETraderServiceType serviceType, string subserviceId, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                traderDict[traderId] = true;
            }
            else
            {
                _servicePurchased[serviceType] = new Dictionary<string, bool>();
                _servicePurchased[serviceType][traderId] = true;
            }

            if (OnTraderServicePurchased != null)
            {
                OnTraderServicePurchased.Invoke(serviceType, subserviceId);
            }
        }

        public void RemovePurchasedService(ETraderServiceType serviceType, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                traderDict[traderId] = false;
            }
        }

        public bool IsServicePurchased(ETraderServiceType serviceType, string traderId)
        {
            if (_servicePurchased.TryGetValue(serviceType, out var traderDict))
            {
                if (traderDict.TryGetValue(traderId, out var result))
                {
                    return result;
                }
            }

            return false;
        }
    }
}
