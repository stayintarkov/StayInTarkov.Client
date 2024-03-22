using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib.Tools;
using UnityEngine;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.SinglePlayer/Utils/TraderServices/LightKeeperServicesManager.cs
    /// Modified by: KWJimWails. Modified to use SIT ModulePatch
    /// </summary>
    internal class LightKeeperServicesManager : MonoBehaviour
    {
        private static ManualLogSource logger;
        GameWorld gameWorld;
        BotsController botsController;

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LightKeeperServicesManager));
            Singleton<LightKeeperServicesManager>.Create(this);

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || TraderServicesManager.Instance == null)
            {
                logger.LogError("[AKI-LKS] GameWorld or TraderServices null");
                Destroy(this);
                return;
            }

            botsController = Singleton<IBotGame>.Instance.BotsController;
            if (botsController == null)
            {
                logger.LogError("[AKI-LKS] BotsController null");
                Destroy(this);
                return;
            }

            TraderServicesManager.Instance.OnTraderServicePurchased += OnTraderServicePurchased;
            logger.LogInfo("[AKI-LKS] LightKeeperServicesManager.Awake");
        }

        private void OnTraderServicePurchased(ETraderServiceType serviceType, string subserviceId)
        {
            switch (serviceType)
            {
                case ETraderServiceType.ExUsecLoyalty:
                    botsController.BotTradersServices.LighthouseKeeperServices.OnFriendlyExUsecPurchased(gameWorld.MainPlayer);
                    break;
                case ETraderServiceType.ZryachiyAid:
                    botsController.BotTradersServices.LighthouseKeeperServices.OnFriendlyZryachiyPurchased(gameWorld.MainPlayer);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (gameWorld == null || botsController == null)
            {
                return;
            }

            if (TraderServicesManager.Instance != null)
            {
                TraderServicesManager.Instance.OnTraderServicePurchased -= OnTraderServicePurchased;
            }
        }
    }
}
