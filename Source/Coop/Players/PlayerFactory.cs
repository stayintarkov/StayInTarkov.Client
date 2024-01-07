using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace StayInTarkov.Coop.Players
{
    internal class PlayerFactory
    {
        public static QuestController GetQuestController(EFT.Profile profile, InventoryController inventoryController)
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

        public static AchievementControllerClass GetAchievementController(EFT.Profile profile, InventoryController inventoryController)
        {
            var aController = new AchievementControllerClass(profile, inventoryController, StayInTarkovHelperConstants.BackEndSession, true);
            aController.Init();
            aController.Run();
            return aController;
        }
    }
}
