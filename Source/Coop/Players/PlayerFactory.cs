using EFT;
using EFT.HealthSystem;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.Controllers;
//using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using static EFT.Player;
using UnityEngine;
using EFT.InventoryLogic;
using Comfort.Common;

namespace StayInTarkov.Coop.Players
{
    internal class PlayerFactory
    {
        public static QuestController GetQuestController(EFT.Profile profile, InventoryControllerClass inventoryController)
        {
            var questController = new QuestController(profile, inventoryController, null, false);
            questController.Init();
            questController.Run();
            return questController;
        }

        public static IStatisticsManager GetStatisticsManager(EFT.Player player)
        {
            IStatisticsManager statsManager = null;
            if (player.IsYourPlayer)
            {
                statsManager = new CoopPlayerStatisticsManager(player.Profile);
                statsManager.Init(player);
            }
            else
            {
                statsManager = new NullStatisticsManager();
            }
            return statsManager;
        }

        public static AchievementControllerClass GetAchievementController(EFT.Profile profile, InventoryControllerClass inventoryController)
        {
            var aController = new AchievementControllerClass(profile, inventoryController, StayInTarkovHelperConstants.BackEndSession, true);
            aController.Init();
            aController.Run();
            return aController;
        }

        public static async Task<EFT.Player>
            Create(int playerId
            , Vector3 position
            , Quaternion rotation
            , string layerName
            , string prefix
            , EPointOfView pointOfView
            , Profile profile
            , bool aiControl
            , EUpdateQueue updateQueue
            , EUpdateMode armsUpdateMode
            , EUpdateMode bodyUpdateMode
            , CharacterControllerSpawner.Mode characterControllerMode
            , Func<float> getSensitivity, Func<float> getAimingSensitivity
            , IFilterCustomization filter
            , AbstractQuestControllerClass questController = null
            , AbstractAchievementControllerClass achievementsController = null
            , bool isYourPlayer = false
            , bool isClientDrone = false)
        {
            return await CoopPlayer.Create(playerId
               , position
               , Quaternion.identity
               ,
               "Player",
               ""
               , EPointOfView.ThirdPerson
               , profile
               , aiControl: aiControl
               , EUpdateQueue.Update
               , EFT.Player.EUpdateMode.Manual
               , EFT.Player.EUpdateMode.Auto
               // Cant use ObservedPlayerMode, it causes the player to fall through the floor and die
               , BackendConfigManager.Config.CharacterController.ObservedPlayerMode
               //, BackendConfigManager.Config.CharacterController.ClientPlayerMode
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
               , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
               , FilterCustomizationClass.Default
               , null
               , isYourPlayer: false
               , isClientDrone: true
               );
        }
    }
}
