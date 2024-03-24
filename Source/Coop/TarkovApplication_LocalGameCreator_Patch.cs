using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Matchmaker;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.Coop
{
    /// <summary>
    /// Created by: Paulov
    /// Paulov: Overwrite and use our own CoopGame instance instead
    /// </summary>
    internal class TarkovApplication_LocalGameCreator_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(typeof(TarkovApplication)).Single(
                x =>

                x.GetParameters().Length >= 2
                && x.GetParameters()[0].ParameterType == typeof(TimeAndWeatherSettings)
                && x.GetParameters()[1].ParameterType == typeof(MatchmakerTimeHasCome.TimeHasComeScreenController)
                );
        }

        public static ISession CurrentSession { get; set; }

        [PatchPrefix]
        public static bool Prefix(TarkovApplication __instance)
        {
            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Prefix");

            if (SITMatchmaking.IsSinglePlayer)
                return true;

            ISession session = __instance.GetClientBackEndSession();
            if (session == null)
            {
                Logger.LogError("Session is NULL. Continuing as Singleplayer.");
                return true;
            }

            CurrentSession = session;

            return false;
        }

        [PatchPostfix]
        public static async Task Postfix(
            Task __result,
           TarkovApplication __instance,
           TimeAndWeatherSettings timeAndWeather,
           MatchmakerTimeHasCome.TimeHasComeScreenController timeHasComeScreenController,
            RaidSettings ____raidSettings,
            InputTree ____inputTree,
            GameDateTime ____localGameDateTime,
            float ____fixedDeltaTime,
            string ____backendUrl
            )
        {
            if (SITMatchmaking.IsSinglePlayer)
                return;

            if (CurrentSession == null)
                return;

            if (____raidSettings == null)
            {
                Logger.LogError("RaidSettings is Null");
                throw new ArgumentNullException("RaidSettings");
            }

            if (timeHasComeScreenController == null)
            {
                Logger.LogError("timeHasComeScreenController is Null");
                throw new ArgumentNullException("timeHasComeScreenController");
            }

            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Postfix");

            LocationSettingsClass.Location location = ____raidSettings.SelectedLocation;

            SITMatchmaking.TimeHasComeScreenController = timeHasComeScreenController;

            //Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Postfix");
            if (Singleton<NotificationManagerClass>.Instantiated)
            {
                Singleton<NotificationManagerClass>.Instance.Deactivate();
            }

            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Postfix: Attempt to get Session");

            ISession session = CurrentSession;
            //IBackEndSession session = ReflectionHelpers.GetFieldOrPropertyFromInstance<IBackEndSession>(__instance, "Session", false);// Profile profile = base.Session.Profile;

            // Get player profile by side
            Profile profile = session.GetProfileBySide(____raidSettings.Side);

            profile.Inventory.Stash = null;
            profile.Inventory.QuestStashItems = null;
            profile.Inventory.DiscardLimits = Singleton<ItemFactory>.Instance.GetDiscardLimits();
            ____raidSettings.RaidMode = ERaidMode.Online;

            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Postfix: Attempt to set Raid Settings");

            await session.SendRaidSettings(____raidSettings);

            if (SITMatchmaking.IsClient)
                timeHasComeScreenController.ChangeStatus(StayInTarkovPlugin.LanguageDictionary["JOINING_COOP_GAME"].ToString());
            else
                timeHasComeScreenController.ChangeStatus(StayInTarkovPlugin.LanguageDictionary["CREATED_COOP_GAME"].ToString());

            await Task.Delay(1000);
            CoopSITGame localGame = CoopSITGame.Create(

            // this is used for testing differences between CoopGame and EFT.LocalGame
            //EFT.LocalGame localGame = (EFT.LocalGame)ReflectionHelpers.GetMethodForType(typeof(EFT.LocalGame), "smethod_6").Invoke(null, 
            //    new object[] {

                ____inputTree
                , profile
                , ____localGameDateTime
                , session.InsuranceCompany
                , MonoBehaviourSingleton<MenuUI>.Instance
                , MonoBehaviourSingleton<CommonUI>.Instance
                , MonoBehaviourSingleton<PreloaderUI>.Instance
                , MonoBehaviourSingleton<GameUI>.Instance
                , ____raidSettings.SelectedLocation
                , timeAndWeather
                , ____raidSettings.WavesSettings
                , ____raidSettings.SelectedDateTime
                , new Callback<ExitStatus, TimeSpan, MetricsClass>((r) =>
                {
                    // target private async void method_46(string profileId, Profile savageProfile, LocationSettingsClass.Location location, Result<ExitStatus, TimeSpan, MetricsClass> result, MatchmakerTimeHasCome.TimeHasComeScreenController timeHasComeScreenController = null)
                    //Logger.LogInfo("Callback Metrics. Invoke method 45");
                    //ReflectionHelpers.GetMethodForType(__instance.GetType(), "method_45").Invoke(__instance, new object[] {
                    //session.Profile.Id, session.ProfileOfPet, ____raidSettings.SelectedLocation, r, timeHasComeScreenController
                    //});

                    ReflectionHelpers.GetAllMethodsForObject(__instance).FirstOrDefault(
                        x =>
                        x.GetParameters().Length >= 5
                        && x.GetParameters()[0].ParameterType == typeof(string)
                        && x.GetParameters()[1].ParameterType == typeof(Profile)
                        && x.GetParameters()[2].ParameterType == typeof(LocationSettingsClass.Location)
                        && x.GetParameters()[3].ParameterType == typeof(Result<ExitStatus, TimeSpan, MetricsClass>)
                        && x.GetParameters()[4].ParameterType == typeof(MatchmakerTimeHasCome.TimeHasComeScreenController)
                        ).Invoke(__instance, new object[] {
                    session.Profile.Id, session.ProfileOfPet, ____raidSettings.SelectedLocation, r, timeHasComeScreenController });

                })
                , ____fixedDeltaTime
                , EUpdateQueue.Update
                , session
                , TimeSpan.FromSeconds(60 * ____raidSettings.SelectedLocation.EscapeTimeLimit)
            //}
            );
            Singleton<AbstractGame>.Create(localGame);
            timeHasComeScreenController.ChangeStatus(StayInTarkovPlugin.LanguageDictionary["CREATED_COOP_GAME"].ToString());


            GetLogger(typeof(TarkovApplication_LocalGameCreator_Patch)).LogDebug("Wait for localGame method 4");

            //var m4task = localGame.method_4(____raidSettings.BotSettings, ____backendUrl, null, new Callback((r) =>
            var m4task = localGame.Run(____raidSettings.BotSettings, ____backendUrl, null, new Callback((r) =>
            {
                using (TokenStarter.StartWithToken("LoadingScreen.LoadComplete"))
                {
                    GetLogger(typeof(TarkovApplication_LocalGameCreator_Patch)).LogDebug("LoadComplete");
                    
                    UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
                    MainMenuController mmc =
                            (MainMenuController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(TarkovApplication), typeof(MainMenuController)).GetValue(__instance);
                    mmc.Unsubscribe();
                    
                    Singleton<GameWorld>.Instance.OnGameStarted();
                    GetLogger(typeof(TarkovApplication_LocalGameCreator_Patch)).LogDebug("OnGameStarted");
                }

            }));

            __result = Task.WhenAll(m4task);
        }
    }
}
