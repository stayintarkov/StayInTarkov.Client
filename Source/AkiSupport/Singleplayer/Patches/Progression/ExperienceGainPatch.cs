using EFT;
using EFT.UI.SessionEnd;
using HarmonyLib;
using System.Linq;
using System.Reflection;

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
            return AccessTools.Method(typeof(SessionResultExperienceCount), nameof(SessionResultExperienceCount.Show), new[] { typeof(Profile), typeof(bool), typeof(ExitStatus) });
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
        private static void PatchPrefix(ref Profile profile, ref bool isOnline)
        {
            Logger.LogInfo("PatchPrefix");
            //profile = CoopPlayerStatisticsManager.Profile;
            isOnline = false;
        }

        [PatchPostfix]
        private static void PatchPostfix(ref bool isOnline)
        {
            isOnline = true;
        }
    }
}
