using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.AkiSupport.Custom.CustomAI
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT/Modules/src/branch/master/project/SPT.Custom/CustomAI/AiHelpers.cs
    /// </summary>
    public static class AiHelpers
    {
        /// <summary>
        /// Bot is a PMC when it has IsStreamerModeAvailable flagged and has a wildspawn type of 'sptBear' or 'sptUsec'
        /// </summary>
        /// <param name="botRoleToCheck">Bots role</param>
        /// <param name="___botOwner_0">Bot details</param>
        /// <returns></returns>
        public static bool BotIsSptPmc(WildSpawnType botRoleToCheck, BotOwner ___botOwner_0)
        {
            if (___botOwner_0.Profile.Info.IsStreamerModeAvailable)
            {
                // PMCs can sometimes have thier role changed to 'assaultGroup' by the client, we need a alternate way to figure out if they're a spt pmc
                return true;
            }

            return (int) botRoleToCheck == WildSpawnTypePrePatcher.sptBearValue || (int) botRoleToCheck == WildSpawnTypePrePatcher.sptUsecValue;
        }

        public static bool BotIsPlayerScav(WildSpawnType role, string nickname)
        {
            if (role == WildSpawnType.assault && nickname.Contains("("))
            {
                // Check bot is pscav by looking for the opening parentheses of their nickname e.g. scavname (pmc name)
                return true;
            }

            return false;
        }

        public static bool BotIsNormalAssaultScav(WildSpawnType role, BotOwner ___botOwner_0)
        {
            // Is assault + no (
            if (!___botOwner_0.Profile.Info.Nickname.Contains("(") && role == WildSpawnType.assault)
            {
                return true;
            }

            return false;
        }
    }

}
