using Comfort.Common;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Progression
{

    /// <summary>
    /// After picking up a quest item, trigger CheckForStatusChange() from the questController to fully update a quest subtasks to show (e.g. `survive and extract item from raid` task)
    /// </summary>
    public class MidRaidQuestChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(Profile), "AddToCarriedQuestItems");
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            Logger.LogDebug($"[MidRaidQuestChangePatch] PatchPostfix");
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError($"[MidRaidQuestChangePatch] gameWorld instance was null");

                return;
            }

            var player = gameWorld.MainPlayer;

            var questController = (QuestController)ReflectionHelpers.GetFieldFromType(player.GetType(), "_questController").GetValue(player);
            if (questController == null) 
            {
				Logger.LogError($"[MidRaidQuestChangePatch] questController instance was null");

				return;
			}

            foreach (var quest in questController.Quests.ToList())
            {
                quest.CheckForStatusChange(true, true);
            }
        }
    }
}