using EFT;
using EFT.Counters;
using EFT.UI.SessionEnd;
using System.Linq;
using System.Reflection;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Networking;
using HarmonyLib;
using System;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Progression
{
    /// <summary>
    /// Fix xp gained value being 0 after a scav raid
    /// </summary>
    public class ScavExperienceGainPatch : ModulePatch
    {
        /// <summary>
        /// Looking for SessionResultExitStatus Show() (private)
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(SessionResultExitStatus),
                nameof(SessionResultExitStatus.Show),
                new[] { typeof(Profile), typeof(PlayerVisualRepresentation), typeof(ESideType), typeof(ExitStatus), typeof(TimeSpan), typeof(ISession), typeof(bool) });
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 7
                && parameters[0].Name == "activeProfile"
                && parameters[1].Name == "lastPlayerState"
                && parameters[2].Name == "side"
                && parameters[3].Name == "exitStatus"
                && parameters[4].Name == "raidTime");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Profile activeProfile,ref EPlayerSide side, ISession session)
        {
            if (activeProfile.Side == EPlayerSide.Savage)
            {
                side = EPlayerSide.Savage; // Also set side to correct value (defaults to usec/bear when playing as scav)
                int xpGainedInSession = activeProfile.Stats.Eft.SessionCounters.GetAllInt(new object[] { CounterTag.Exp });
                activeProfile.Stats.Eft.TotalSessionExperience = (int)(xpGainedInSession * activeProfile.Stats.Eft.SessionExperienceMult * activeProfile.Stats.Eft.ExperienceBonusMult);
            }

            return true; // Always do original method
        }
    }
}