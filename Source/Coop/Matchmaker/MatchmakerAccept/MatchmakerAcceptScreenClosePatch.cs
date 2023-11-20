using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Matchmaker
{
    public class MatchmakerAcceptScreenClosePatch : ModulePatch
    {
        static BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static Type GetThisType()
        {
            return StayInTarkovHelperConstants.EftTypes
                 .Single(x => x == typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen));
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Close";

            return GetThisType().GetMethods(privateFlags).First(x => x.Name == methodName);

        }


        [PatchPrefix]
        private static bool PatchPrefix(
            EFT.UI.Matchmaker.MatchMakerAcceptScreen __instance
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenClosePatch.PatchPrefix");
            return true;

        }

    }


}
