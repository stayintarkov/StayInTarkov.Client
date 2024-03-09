using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using StayInTarkov.Coop.Components;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Matchmaker
{
    public class MatchmakerAcceptScreenShowPatch : ModulePatch
    {
        public static Type GetThisType()
        {
            return StayInTarkovHelperConstants.EftTypes
                 .Single(x => x == typeof(MatchMakerAcceptScreen));
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Show";

            return GetThisType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == methodName && x.GetParameters()[0].Name == "session");

        }

        private static DateTime LastClickedTime { get; set; } = DateTime.MinValue;

        private static GameObject MatchmakerObject { get; set; }

        [PatchPrefix]
        private static void Pre(
            ref ISession session,
            ref RaidSettings raidSettings,
            Profile ___profile_0,
            MatchMakerAcceptScreen __instance,
            DefaultUIButton ____acceptButton,
            DefaultUIButton ____backButton,
            MatchMakerPlayerPreview ____playerModelView
            )
        {
            //Logger.LogDebug("MatchmakerAcceptScreenShow.PatchPrefix");

            if (MatchmakerObject == null)
                MatchmakerObject = new GameObject("MatchmakerObject");
            
            // Raid Mode needs to be local for Scav raids
            if (raidSettings.Side == ESideType.Savage)
                raidSettings.RaidMode = ERaidMode.Local;

            var sitMatchMaker = MatchmakerObject.GetOrAddComponent<SITMatchmakerGUIComponent>();
            sitMatchMaker.Profile = ___profile_0;
            sitMatchMaker.RaidSettings = raidSettings;
            sitMatchMaker.OriginalAcceptButton = ____acceptButton;
            sitMatchMaker.OriginalBackButton = ____backButton;
            sitMatchMaker.MatchMakerPlayerPreview = ____playerModelView;


            //var rs = raidSettings;
            //____acceptButton.OnClick.AddListener(() =>
            //{
            //    if (LastClickedTime < DateTime.Now.AddSeconds(-10))
            //    {
            //        LastClickedTime = DateTime.Now;

            //        //Logger.LogDebug("MatchmakerAcceptScreenShow.PatchPrefix:Clicked");
            //        if (MatchmakerAcceptPatches.CheckForMatch(rs, out string returnedJson))
            //        {
            //            Logger.LogDebug(returnedJson);
            //            JObject result = JObject.Parse(returnedJson);
            //            var groupId = result["ServerId"].ToString();
            //            Matchmaker.MatchmakerAcceptPatches.SetGroupId(groupId);
            //            MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
            //            GC.Collect();
            //            GC.WaitForPendingFinalizers();
            //            GC.Collect();
            //        }
            //        else
            //        {
            //            MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId, rs);
            //        }
            //    }
            //});
        }


        [PatchPostfix]
        private static void Post(
            ref ISession session,
            ref RaidSettings raidSettings,
            Profile ___profile_0,
            MatchMakerAcceptScreen __instance,
            DefaultUIButton ____acceptButton
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPostfix");

            // ------------------------------------------
            // Keep an instance for other patches to work
            SITMatchmaking.MatchMakerAcceptScreenInstance = __instance;
            // ------------------------------------------
            SITMatchmaking.Profile = ___profile_0;
        }
    }


}
