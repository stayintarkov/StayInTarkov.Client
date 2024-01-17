using System.Linq;
using System.Reflection;
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
            // If extracted alive subtract 300 from session experience to account for extract bonus, if below 200 it's a run through
            if (exitStatus == ExitStatus.Survived)
            {
                var xpGained = activeProfile.EftStats.TotalSessionExperience;
                if (xpGained > 300) xpGained -= 300;
                if (xpGained < 200) exitStatus = ExitStatus.Runner;
            }
        }
    }
}