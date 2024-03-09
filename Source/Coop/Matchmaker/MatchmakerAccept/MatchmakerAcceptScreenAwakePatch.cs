using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Matchmaker
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

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen), "Awake");
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            EFT.UI.Matchmaker.MatchMakerAcceptScreen __instance
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenAwakePatch.PatchPrefix");
            SITMatchmaking.MatchMakerAcceptScreenInstance = __instance;
            return true;
        }

    }
}









