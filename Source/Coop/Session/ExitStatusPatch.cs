using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.UI.SessionEnd;

namespace StayInTarkov.Coop.Session
{
    public class ExitStatusPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(SessionResultExitStatus);
            var desiredMethod = ReflectionHelpers.GetAllMethodsForType(desiredType).FirstOrDefault(IsTargetMethod);

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 7
                   && parameters[0].Name == "activeProfile"
                   && parameters[1].Name == "lastPlayerState"
                   && parameters[2].Name == "side"
                   && parameters[3].Name == "exitStatus";
        }

        [PatchPrefix]
        private static void PatchPrefix(ref Profile activeProfile, ref ExitStatus exitStatus)
        {
            Logger.LogInfo("PatchPrefix");
            if (exitStatus == ExitStatus.Survived)
            {
                var matchEnd = Singleton<Config4>.Instance.Experience.MatchEnd;
                var xpGained = activeProfile.EftStats.TotalSessionExperience;
                if (xpGained > matchEnd.SurvivedReward) xpGained -= matchEnd.SurvivedReward;
                if (xpGained < matchEnd.SurvivedExpRequirement) exitStatus = ExitStatus.Runner;
            }
        }
    }
}