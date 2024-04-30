using System.Reflection;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using HarmonyLib;
using TMPro;
using StayInTarkov;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Debugging/AkiDebuggingPlugin.cs
    /// </summary>
    public class EndRaidDebug : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderCard), nameof(TraderCard.method_0));
        }

        [PatchPrefix]
        private static bool PatchPreFix(ref LocalizedText ____nickName, ref TMP_Text ____standing,
            ref RankPanel ____rankPanel, ref Profile.TraderInfo ___traderInfo_0)
        {
            if (____nickName.LocalizationKey == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] _nickName.LocalizationKey was null");
            }

            if (____standing.text == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] _standing.text was null");
            }

            if (____rankPanel == null)
            {
                Logger.Log(LogLevel.Error, "[AKI] _rankPanel was null, skipping method_0");
                return false; // skip original
            }

            if (___traderInfo_0?.LoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] ___traderInfo_0 or ___traderInfo_0.LoyaltyLevel was null");
            }

            if (___traderInfo_0?.MaxLoyaltyLevel == null)
            {
                ConsoleScreen.LogError("This Shouldn't happen!! Please report this in discord");
                Logger.Log(LogLevel.Error, "[AKI] ___traderInfo_0 or ___traderInfo_0.MaxLoyaltyLevel was null");
            }

            return true;
        }
    }
}