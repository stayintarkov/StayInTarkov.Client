using EFT.Quests;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Quests
{
    /**
     * Lightkeeper quests change their state in-raid, and will change to the `AppearStatus` of the quest once
     * the AvailableAfter time has been hit. This defaults to `Locked`, but should actually be `AvailableForStart`
     * 
     * So if we get a quest state change from `AvailableAfter` to `Locked`, we should actually change to `AvailableForStart`
     */
    public class InRaidQuestAvailablePatch : ModulePatch
    {
        private static PropertyInfo _questStatusProperty;

        protected override MethodBase GetTargetMethod()
        {
            var targetType = StayInTarkovHelperConstants.EftTypes.FirstOrDefault(IsTargetType);
            var targetMethod = AccessTools.Method(targetType, "SetStatus");

            _questStatusProperty = AccessTools.Property(targetType, "QuestStatus");

            Logger.LogDebug($"{this.GetType().Name} Type: {targetType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {targetMethod?.Name}");
            Logger.LogDebug($"{this.GetType().Name} QuestStatus: {_questStatusProperty?.Name}");

            return targetMethod;
        }

        private bool IsTargetType(Type type)
        {
            if (type.GetProperty("StatusTransition") != null &&
                type.GetProperty("IsChangeAllowed") != null &&
                type.GetProperty("NeedCountdown") == null)
            {
                return true;
            }

            return false;
        }

        [PatchPrefix]
        private static void PatchPrefix(object __instance, ref EQuestStatus status)
        {
            var currentStatus = (EQuestStatus)_questStatusProperty.GetValue(__instance);

            if (currentStatus == EQuestStatus.AvailableAfter && status == EQuestStatus.Locked)
            {
                status = EQuestStatus.AvailableForStart;
            }
        }
    }
}
