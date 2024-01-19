using EFT;
using EFT.UI.SessionEnd;
using System.Linq;
using System.Reflection;
using Comfort.Common;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Progression
{
    /// <summary>
    /// Credit SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/Progression/ExperienceGainPatch.cs
    /// </summary>
    public class ExperienceGainPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(SessionResultExperienceCount);
            var desiredMethod = ReflectionHelpers.GetAllMethodsForType(desiredType).FirstOrDefault(IsTargetMethod);

            //Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            //Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "profile"
                && parameters[1].Name == "isOnline"
                && parameters[2].Name == "exitStatus"
                && parameters[1].ParameterType == typeof(bool);
        }

        [PatchPrefix]
        private static void PatchPrefix(ref Profile profile, ref bool isOnline, ref ExitStatus exitStatus)
        {
            Logger.LogInfo("PatchPrefix");            
            if (exitStatus == ExitStatus.Survived)
            {
                var matchEnd = Singleton<Config4>.Instance.Experience.MatchEnd;
                var xpGained = profile.EftStats.TotalSessionExperience;
                if (xpGained > matchEnd.SurvivedReward) xpGained -= matchEnd.SurvivedReward;
                if (xpGained < matchEnd.SurvivedExpRequirement) exitStatus = ExitStatus.Runner;
            }
            //profile = CoopPlayerStatisticsManager.Profile;
            isOnline = true;
        }

        [PatchPostfix]
        private static void PatchPostfix(ref bool isOnline)
        {
            isOnline = true;
        }
    }
}
