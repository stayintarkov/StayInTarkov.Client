using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Matchmaker
{
    public class MatchmakerAcceptScreenAwakePatch : ModulePatch
    {
        [Serializable]
        private class ServerStatus
        {
            [JsonProperty("ip")]
            public string ip { get; set; }

            [JsonProperty("status")]
            public string status { get; set; }
        }

        static BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static Type GetThisType()
        {
            return StayInTarkovHelperConstants.EftTypes
                 .Single(x => x == typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen));
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Awake";

            return GetThisType().GetMethods(privateFlags).First(x => x.Name == methodName);

        }

        [PatchPrefix]
        private static bool PatchPrefix(
            EFT.UI.Matchmaker.MatchMakerAcceptScreen __instance
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenAwakePatch.PatchPrefix");
            MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance = __instance;
            return true;
        }

    }
}









