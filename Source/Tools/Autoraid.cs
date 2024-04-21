using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using EFT.UI.Screens;
using EFT.UI.SessionEnd;
using JsonType;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Health;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StayInTarkov.Tools
{
    public class Autoraid : MonoBehaviour
    {
        public static bool Running = false;

        public void Start()
        {
            ScreenManager.Instance.OnScreenChanged += OnScreenChanged;
        }

        public void OnDestroy()
        {
            Running = false;
        }

        public static bool Requested()
        {
            return Environment.GetCommandLineArgs().FirstOrDefault(x => x == "-autoraid") != null;
        }

        private void OnScreenChanged(EEftScreenType x)
        {
            if (x == EEftScreenType.MainMenu)
            {
                Running = true;
                ScreenManager.Instance.OnScreenChanged -= OnScreenChanged;

                IEnumerator autoraid()
                {
                    try
                    {
                        var i = 3;

                        while (i-- > 0)
                        {
                            var screens = (Dictionary<EEftScreenType, UIScreen>)ReflectionHelpers.GetFieldFromType(typeof(ScreenManager), "dictionary_0").GetValue(ScreenManager.Instance);

                            {
                                yield return new WaitUntil(() => screens.ContainsKey(EEftScreenType.MainMenu) && screens[EEftScreenType.MainMenu].gameObject.activeSelf);
                                var playButton = new WaitForButton<MenuScreen, DefaultUIButton>(screens[EEftScreenType.MainMenu], "_playButton");
                                yield return playButton;
                                playButton.Button.OnClick.Invoke();
                            }

                            {
                                var pmcButton = new WaitForButton<MatchMakerSideSelectionScreen, UnityEngine.UI.Button>(screens[EEftScreenType.SelectRaidSide], "_pmcBigButton");
                                yield return pmcButton;
                                pmcButton.Button.OnPointerClick(new PointerEventData(EventSystem.current)
                                {
                                    button = PointerEventData.InputButton.Left
                                });
                                var nextButton = new WaitForButton<MatchMakerSideSelectionScreen, DefaultUIButton>(screens[EEftScreenType.SelectRaidSide], "_nextButton");
                                yield return nextButton;
                                nextButton.Button.OnClick.Invoke();
                            }

                            // remove as much non-determinism as we can
                            var app = StayInTarkovHelperConstants.GetMainApp();
                            var botSettings = new BotControllerSettings(isScavWars: false, EBotAmount.Horde, EBossType.AsOnline);
                            var locationSettings = app.Session.LocationSettings;
                            var location = locationSettings.locations.Single(x => x.Value.Name == "Customs").Value;
                            location.BotSpawnTimeOffMin = 1;
                            location.BotSpawnTimeOffMax = 1;
                            location.BotSpawnPeriodCheck = 10;
                            location.BotStart = 0;
                            location.BotMax = 30;
                            foreach (var x in location.BossLocationSpawn)
                            {
                                x.BossChance = 100;
                                x.ForceSpawn = true;
                                x.IgnoreMaxBots = true;
                            }
                            var mmc = (MainMenuController)ReflectionHelpers.GetFieldFromType(typeof(TarkovApplication), "gclass1833_0").GetValue(app);
                            var raidSettings = (RaidSettings)ReflectionHelpers.GetFieldFromType(typeof(MainMenuController), "raidSettings_0").GetValue(mmc);
                            raidSettings.Apply(new RaidSettings(
                                side: ESideType.Pmc,
                                selectedDateTime: EDateTime.CURR,
                                raidMode: ERaidMode.Local,
                                timeAndWeatherSettings: new TimeAndWeatherSettings(
                                    randomTime: false,
                                    randomWeather: false,
                                    cloudinessType: 0,
                                    rainType: 0,
                                    windSpeed: 0,
                                    fogType: 0,
                                    timeFlowType: 4,
                                    hourOfDay: -1
                                ),
                                locationSettings: locationSettings
                            )
                            {
                                BotSettings = botSettings,
                                SelectedLocation = location
                            });
                            var readyButton = new WaitForButton<MatchMakerSelectionLocationScreen, DefaultUIButton>(screens[EEftScreenType.SelectLocation], "_acceptButton");
                            yield return readyButton;
                            readyButton.Button.OnClick.Invoke();

                            {
                                var nextButton = new WaitForButton<MatchmakerOfflineRaidScreen, DefaultUIButton>(screens[EEftScreenType.OfflineRaid], "_nextButtonSpawner");
                                yield return nextButton;
                                nextButton.Button.OnClick.Invoke();
                            }

                            {
                                var nextButton = new WaitForButton<MatchmakerInsuranceScreen, DefaultUIButton>(screens[EEftScreenType.Insurance], "_nextButton");
                                yield return nextButton;
                                nextButton.Button.OnClick.Invoke();
                            }

                            {
                                yield return new WaitUntil(() => MatchmakerAcceptScreenShowPatch.MatchmakerObject?.GetComponent<SITMatchmakerGUIComponent>() != null);
                                var guiComp = MatchmakerAcceptScreenShowPatch.MatchmakerObject.GetComponent<SITMatchmakerGUIComponent>();
                                guiComp.HostSoloRaidAndJoin(ESITProtocol.PeerToPeerUdp, EBotAmount.High);
                            }

                            if (i > 0)
                            {
                                yield return new WaitForSeconds(300.0f);
                                Singleton<ISITGame>.Instance.Stop(SITMatchmaking.Profile.ProfileId, ExitStatus.Survived, "");

                                {
                                    yield return new WaitUntil(
                                        () => screens.ContainsKey(EEftScreenType.ExitStatus) && screens[EEftScreenType.ExitStatus].gameObject.activeSelf);
                                    var nextButton = new WaitForButton<SessionResultExitStatus, DefaultUIButton>(screens[EEftScreenType.ExitStatus], "_nextButton");
                                    yield return nextButton;
                                    nextButton.Button.OnClick.Invoke();
                                }

                                {
                                    var nextButton = new WaitForButton<SessionResultKillList, DefaultUIButton>(screens[EEftScreenType.KillList], "_nextButton");
                                    yield return nextButton;
                                    nextButton.Button.OnClick.Invoke();
                                }

                                {
                                    var nextButton = new WaitForButton<SessionResultStatistics, DefaultUIButton>(screens[EEftScreenType.SessionStatistics], "_nextButton");
                                    yield return nextButton;
                                    nextButton.Button.OnClick.Invoke();
                                }

                                {
                                    var nextButton = new WaitForButton<SessionResultExperienceCount, DefaultUIButton>(screens[EEftScreenType.SessionExperience], "_nextButton");
                                    yield return nextButton;
                                    nextButton.Button.OnClick.Invoke();
                                }

                                {
                                    var profile = app.Session.Profile;
                                    var ic = new InventoryController(app.Session, profile, profile.Id);
                                    var hc = new HealthControllerClass(profile.Health, ic, profile.Skills, regeneration: true);
                                    if (HealthTreatmentScreen.IsAvailable(profile, hc, app.Session.Medic.Info))
                                    {
                                        var nextButton = new WaitForButton<HealthTreatmentScreen, DefaultUIButton>(screens[EEftScreenType.HealthTreatment], "_nextButton");
                                        yield return nextButton;
                                        nextButton.Button.OnClick.Invoke();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        Destroy(this);
                    }
                }

                StartCoroutine(autoraid());
            }
        }
    }

    class WaitForButton<T, U>(UIScreen screen, string fieldName) : CustomYieldInstruction where T : UIScreen where U : class?
    {
        public U Button;

        public override bool keepWaiting
        {
            get
            {
                Button = (U)ReflectionHelpers.GetFieldFromType(typeof(T), fieldName).GetValue(screen);
                return Button == null;
            }
        }
    }
}
