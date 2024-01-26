﻿using BepInEx.Logging;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StayInTarkov.Configuration;
using UnityEngine;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;

namespace StayInTarkov.Coop.Components
{
    internal class SITMatchmakerGUIComponent : MonoBehaviour
    {
        private UnityEngine.Rect windowRect = new(20, 20, 120, 50);
        private UnityEngine.Rect windowInnerRect { get; set; } = new(20, 20, 120, 50);
        private GUIStyle styleBrowserRaidLabel { get; } = new GUIStyle();
        private GUIStyle styleBrowserRaidRow { get; } = new GUIStyle() { };
        private GUIStyle styleBrowserRaidLink { get; } = new GUIStyle();

        private GUIStyle styleBrowserWindow { get; set; }

        private GUIStyleState styleStateBrowserBigButtonsNormal { get; } = new GUIStyleState()
        {
            textColor = Color.white
        };
        private GUIStyle styleBrowserBigButtons { get; set; }

        public RaidSettings RaidSettings { get; internal set; }
        public DefaultUIButton OriginalBackButton { get; internal set; }
        public DefaultUIButton OriginalAcceptButton { get; internal set; }

        private Task GetMatchesTask { get; set; }

        private Dictionary<string, object>[] m_Matches { get; set; }

        private CancellationTokenSource m_cancellationTokenSource;

        private bool StopAllTasks = false;

        private bool showPasswordField = false;

        private string passwordInput = "";
        private string passwordClientInput = "";

        private int botAmountInput = 0;
        private int botDifficultyInput = 0;

        private string[] BotAmountStringOptions = new string[] { "AsOnline", "None", "Low", "Medium", "High", "Horde" };
        private string[] BotDifficultyStringOptions = new string[] { "AsOnline", "Easy", "Medium", "Hard", "Impossible", "Random" };

        private bool BotBossesEnabled = true;

        private const float verticalSpacing = 10f;

        private ManualLogSource Logger { get; set; }
        public MatchMakerPlayerPreview MatchMakerPlayerPreview { get; internal set; }

        //public Canvas Canvas { get; set; }
        public Profile Profile { get; internal set; }
        public Rect hostGameWindowInnerRect { get; private set; }

        #region Window Determination

        private bool showHostGameWindow { get; set; }
        private bool showServerBrowserWindow { get; set; } = true;
        private bool showErrorMessageWindow { get; set; } = false;
        private bool showPasswordRequiredWindow { get; set; } = false;

        private string pendingServerId = "";

        #endregion

        #region Unity

        void Start()
        {
            // Setup Logger
            Logger = BepInEx.Logging.Logger.CreateLogSource("SIT Matchmaker GUI");
            Logger.LogInfo("Start");
            //// Get Canvas
            //Canvas = GameObject.FindObjectOfType<Canvas>();
            //if (Canvas != null)
            //{
            //    Logger.LogInfo("Canvas found");
            //    foreach (Transform b in Canvas.GetComponents<Transform>())
            //    {
            //        Logger.LogInfo(b);
            //    }
            //    //Canvas.GetComponent<UnityEngine.GUIText>();
            //}

            // Create background Texture
            Texture2D texture2D = new(128, 128);
            texture2D.Fill(Color.black);
            styleStateBrowserBigButtonsNormal.background = texture2D;
            styleStateBrowserBigButtonsNormal.textColor = Color.black;
            //styleStateBrowserWindowNormal.background = texture2D;
            //styleStateBrowserWindowNormal.textColor = Color.white;

            // Create Skin for Window
            //GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
            //skin.window = new GUIStyle();
            //skin.window.alignment = TextAnchor.MiddleLeft;
            //skin.window.normal = styleStateBrowserWindowNormal;

            m_cancellationTokenSource = new CancellationTokenSource();
            styleBrowserBigButtons = new GUIStyle()
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = styleStateBrowserBigButtonsNormal,
                active = styleStateBrowserBigButtonsNormal,
                hover = styleStateBrowserBigButtonsNormal,
            };

            styleBrowserWindow = new GUIStyle();
            styleBrowserWindow.active = styleBrowserWindow.normal;
            styleBrowserWindow.onActive = styleBrowserWindow.onNormal;

            GetMatches();
            StartCoroutine(ResolveMatches());
            DisableBSGButtons();

            var previewsPanel = GameObject.Find("PreviewsPanel");
            if (previewsPanel != null)
            {
                var previewsPanelRect = previewsPanel.GetComponent<RectTransform>();
                previewsPanelRect.position = new Vector3(400, 300, 0);
            }


            var playerImage = GameObject.Find("PlayerImage");
            if (playerImage != null)
            {
                var playerImageRect = playerImage.GetComponent<RectTransform>();
                playerImageRect.localScale = new Vector3(1.4f, 1.4f, 0);
            }
        }

        void OnDestroy()
        {
            if (m_cancellationTokenSource != null)
                m_cancellationTokenSource.Cancel();


            StopAllTasks = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DestroyThis();
            }

        }

        void OnGUI()
        {
            // Define the proportions for the main window and the host game window (same size)
            var windowWidthFraction = 0.4f;
            var windowHeightFraction = 0.4f;

            //Make the window slightly bigger on smaller screens so our elements fit
            if(Screen.height <= 1000)
            {
                windowWidthFraction = 0.55f;
                windowHeightFraction = 0.55f;
            }

            // Calculate the position and size of the main window
            var windowWidth = Screen.width * windowWidthFraction;
            var windowHeight = Screen.height * windowHeightFraction;
            var windowX = (Screen.width - windowWidth) / 2;
            var windowY = (Screen.height - windowHeight) / 2;

            //Set the window to be slightly higher on smaller screens to compensate for the window being bigger
            if (Screen.height <= 1000)
                windowY = (Screen.height - windowHeight) / 4;

            // Create the main window rectangle
            windowRect = new UnityEngine.Rect(windowX, windowY, windowWidth, windowHeight);
            var serverBrowserRect = new UnityEngine.Rect(windowX, windowY, windowWidth, windowHeight);

            if (showServerBrowserWindow)
            {
                windowInnerRect = GUI.Window(0, serverBrowserRect, DrawBrowserWindow, "");

                // Calculate the position for the "Host Game" and "Play Single Player" buttons
                var buttonWidth = 250;
                var buttonHeight = 50;
                var buttonX = (Screen.width - buttonWidth * 2 - 10) / 2;
                var buttonY = Screen.height * 0.75f - buttonHeight;

                // Define a GUIStyle for Host Game and Play single player
                GUIStyle gamemodeButtonStyle = new(GUI.skin.button);
                gamemodeButtonStyle.fontSize = 24;
                gamemodeButtonStyle.fontStyle = FontStyle.Bold;

                // Create "Host Game" button
                if (GUI.Button(new UnityEngine.Rect(buttonX, buttonY, buttonWidth, buttonHeight), StayInTarkovPlugin.LanguageDictionary["HOST_RAID"], gamemodeButtonStyle))
                {
                    showServerBrowserWindow = false;
                    showHostGameWindow = true;
                }

                // Create "Play Single Player" button next to the "Host Game" button
                // Creates a hosted game with 1 player if the "Quick Start Solo" option is enabled
                buttonX += buttonWidth + 10;
                if (!PluginConfigSettings.Instance.CoopSettings.QuickStartSolo)
                {
                    if (GUI.Button(new UnityEngine.Rect(buttonX, buttonY, buttonWidth, buttonHeight), StayInTarkovPlugin.LanguageDictionary["PLAY_SINGLE_PLAYER"], gamemodeButtonStyle))
                    {
                        FixesHideoutMusclePain();
                        MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                        OriginalAcceptButton.OnClick.Invoke();
                        DestroyThis();
                    }
                }
                else
                {
                    if (GUI.Button(new UnityEngine.Rect(buttonX, buttonY, buttonWidth, buttonHeight), StayInTarkovPlugin.LanguageDictionary["SOLO_QUICKSTART"], gamemodeButtonStyle))
                    {
                        FixesHideoutMusclePain();
                        RaidSettings.BotSettings.BotAmount = EBotAmount.AsOnline;
                        RaidSettings.WavesSettings.BotAmount = EBotAmount.AsOnline;
                        RaidSettings.WavesSettings.BotDifficulty = EBotDifficulty.AsOnline;
                        RaidSettings.WavesSettings.IsBosses = true;
                        MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings, passwordInput);
                        OriginalAcceptButton.OnClick.Invoke();
                        DestroyThis();
                        AkiBackendCommunication.Instance.WebSocketCreate(MatchmakerAcceptPatches.Profile);
                    }
                }
            }
            else if (showHostGameWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawHostGameWindow, StayInTarkovPlugin.LanguageDictionary["HOST_RAID"]);
            }

            // Handle the "Back" Button
            if (showServerBrowserWindow || showHostGameWindow)
            {
                // Calculate the vertical position
                var backButtonX = (Screen.width - 200) / 2;
                var backButtonY = Screen.height * 0.95f - 40;

                // Define a GUIStyle for the "Back" button with larger and bold text
                GUIStyle buttonStyle = new(GUI.skin.button);
                buttonStyle.fontSize = 24;
                buttonStyle.fontStyle = FontStyle.Bold;

                if (GUI.Button(new UnityEngine.Rect(backButtonX, backButtonY, 200, 40), StayInTarkovPlugin.LanguageDictionary["BACK"], buttonStyle))
                {
                    // Handle the "Back" button click
                    if (showServerBrowserWindow)
                    {
                        OriginalBackButton.OnClick.Invoke();
                        DestroyThis();
                        AkiBackendCommunication.Instance.WebSocketClose();

                    }
                    else if (showHostGameWindow)
                    {
                        // Add logic to go back to the main menu or previous screen
                        showServerBrowserWindow = true;
                        showHostGameWindow = false;
                    }
                }
            }

            if (showErrorMessageWindow)
            {
                showHostGameWindow = false;
                showServerBrowserWindow = false;
                showPasswordRequiredWindow = false;

                windowInnerRect = GUI.Window(0, windowRect, DrawWindowErrorMessage, "Error Message");
            }

            if (showPasswordRequiredWindow)
            {
                showHostGameWindow = false;
                showServerBrowserWindow = false;

                windowInnerRect = GUI.Window(0, windowRect, DrawPasswordRequiredWindow, "Password required");
            }
        }

        #endregion

        private void DisableBSGButtons()
        {
            OriginalAcceptButton.gameObject.SetActive(false);
            OriginalAcceptButton.enabled = false;
            OriginalAcceptButton.Interactable = false;
            OriginalBackButton.gameObject.SetActive(false);
            OriginalBackButton.enabled = false;
            OriginalBackButton.Interactable = false;

        }

        void GetMatches()
        {
            CancellationToken ct = m_cancellationTokenSource.Token;
            GetMatchesTask = Task.Run(async () =>
            {
                while (!StopAllTasks)
                {
                    var result = await AkiBackendCommunication.Instance.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/getAllForLocation", RaidSettings.ToJson(), timeout: 4000, debug: false);
                    if (result != null)
                    {
                        m_Matches = result;
                    }

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(7000);

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, ct);
        }

        IEnumerator ResolveMatches()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
            }
        }

        string ErrorMessage { get; set; }

        /// <summary>
        /// TODO: Finish this on Error Window
        /// </summary>
        /// <param name="windowID"></param>
        void DrawWindowErrorMessage(int windowID)
        {
            if (!showErrorMessageWindow)
                return;

            GUI.Label(new UnityEngine.Rect(20, 20, 200, 200), ErrorMessage);

            if (GUI.Button(new UnityEngine.Rect(20, windowInnerRect.height - 90, windowInnerRect.width - 40, 45), "Close"))
            {
                showErrorMessageWindow = false;
                showServerBrowserWindow = true;
            }
        }

        void DrawPasswordRequiredWindow(int windowID)
        {
            if (!showPasswordRequiredWindow)
                return;

            var halfWindowWidth = windowInnerRect.width / 2;
            var halfWindowHeight = windowInnerRect.height / 2;

            var PasswordTextWidth = GUI.skin.label.CalcSize(new GUIContent("Enter password")).x;

            var textX = halfWindowWidth - PasswordTextWidth / 2;

            GUI.Label(new UnityEngine.Rect(textX, halfWindowHeight - 100, PasswordTextWidth, 30), "Enter password");

            var passwordFieldWidth = 200;
            var passwordFieldX = halfWindowWidth - passwordFieldWidth / 2;

            passwordClientInput = GUI.PasswordField(new UnityEngine.Rect(passwordFieldX, halfWindowHeight - 50, 200, 30), passwordClientInput, '*', 25);

            var buttonX = halfWindowWidth - PasswordTextWidth / 2;

            if (GUI.Button(new UnityEngine.Rect(buttonX - 60, halfWindowHeight, 100, 40), "Back"))
            {
                showPasswordRequiredWindow = false;
                showServerBrowserWindow = true;
            }

            if (GUI.Button(new UnityEngine.Rect(buttonX + 60, halfWindowHeight, 100, 40), "Join"))
            {
                JoinMatch(MatchmakerAcceptPatches.Profile.ProfileId, pendingServerId, passwordClientInput);
            }
        }

        void DrawBrowserWindow(int windowID)
        {
            // Define column labels
            // Use the Language Dictionary
            string[] columnLabels = {
                StayInTarkovPlugin.LanguageDictionary["SERVER"]
                , StayInTarkovPlugin.LanguageDictionary["PLAYERS"]
                , StayInTarkovPlugin.LanguageDictionary["LOCATION"]
                , StayInTarkovPlugin.LanguageDictionary["PASSWORD"]
            };


            // Define the button style
            GUIStyle buttonStyle = new(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(6, 6, 6, 6);

            // Define the label style
            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = Color.white;

            // Calculate the number of rows and columns
            int numRows = 7;
            int numColumns = columnLabels.Length;

            // Calculate cell width and height
            float cellWidth = windowInnerRect.width / (numColumns + 1);
            float cellHeight = (windowInnerRect.height - 40) / numRows;

            // Calculate the vertical positions for lines and labels
            float topSeparatorY = 15;
            float middleSeparatorY = topSeparatorY + cellHeight - 7;
            float bottomSeparatorY = topSeparatorY + numRows * cellHeight;

            // Calculate the width of the separator
            float separatorWidth = 2;

            // Draw the first horizontal line at the top
            GUI.DrawTexture(new UnityEngine.Rect(10, topSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

            // Draw the second horizontal line under Server, Players, and Location
            GUI.DrawTexture(new UnityEngine.Rect(10, middleSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

            // Draw the third horizontal line at the bottom
            GUI.DrawTexture(new UnityEngine.Rect(10, bottomSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

            // Draw vertical separator lines
            for (int col = 1; col < numColumns + 1; col++)
            {
                float separatorX = col * cellWidth - separatorWidth / 2;
                GUI.DrawTexture(new UnityEngine.Rect(separatorX - 2, topSeparatorY, separatorWidth, bottomSeparatorY - topSeparatorY), Texture2D.grayTexture);
            }

            // Draw column labels at the top
            for (int col = 0; col < numColumns; col++)
            {
                float cellX = col * cellWidth + separatorWidth / 2;
                GUI.Label(new UnityEngine.Rect(cellX, topSeparatorY + 5, cellWidth - separatorWidth, 25), columnLabels[col], labelStyle);
            }

            // Reset the GUI.backgroundColor to its original state
            GUI.backgroundColor = Color.white;

            if (m_Matches != null)
            {
                var index = 0;
                var yPosOffset = 60;

                foreach (var match in m_Matches)
                {
                    var yPos = yPosOffset + index * (cellHeight + 5);

                    // Display Host Name with "Raid" label
                    GUI.Label(new UnityEngine.Rect(10, yPos, cellWidth - separatorWidth, cellHeight), $"{match["HostName"].ToString()} Raid", labelStyle);

                    // Display Player Count
                    GUI.Label(new UnityEngine.Rect(cellWidth, yPos, cellWidth - separatorWidth, cellHeight), match["PlayerCount"].ToString(), labelStyle);

                    // Display Location
                    GUI.Label(new UnityEngine.Rect(cellWidth * 2, yPos, cellWidth - separatorWidth, cellHeight), match["Location"].ToString(), labelStyle);

                    // Display Password Locked
                    GUI.Label(new UnityEngine.Rect(cellWidth * 3, yPos, cellWidth - separatorWidth, cellHeight), bool.Parse(match["IsPasswordLocked"].ToString()) ? "Yes" : "", labelStyle);

                    // Calculate the width of the combined server information (Host Name, Player Count, Location)
                    var serverInfoWidth = cellWidth * 3 - separatorWidth * 2;

                    // Create "Join" button for each match on the next column
                    if (GUI.Button(new UnityEngine.Rect(cellWidth * 4 + separatorWidth / 2 + 15, yPos + (cellHeight * 0.3f), cellWidth * 0.8f, cellHeight * 0.5f), StayInTarkovPlugin.LanguageDictionary["JOIN"], buttonStyle))
                    {
                        // Perform actions when the "Join" button is clicked
                        JoinMatch(MatchmakerAcceptPatches.Profile.ProfileId, match["ServerId"].ToString());
                    }

                    index++;
                }
            }
        }

        void JoinMatch(string profileId, string serverId, string password = "")
        {
            if (MatchmakerAcceptPatches.JoinMatch(RaidSettings, profileId, serverId, password, out string returnedJson, out string errorMessage))
            {
                Logger.LogDebug(returnedJson);
                JObject result = JObject.Parse(returnedJson);
                MatchmakerAcceptPatches.SetGroupId(result["serverId"].ToString());
                MatchmakerAcceptPatches.SetTimestamp(long.Parse(result["timestamp"].ToString()));
                MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers = int.Parse(result["expectedNumberOfPlayers"].ToString());


                FixesHideoutMusclePain();
                DestroyThis();
                OriginalAcceptButton.OnClick.Invoke();
            }
            else
            {
                if (errorMessage == "passwordRequired")
                {
                    showPasswordRequiredWindow = true;
                    pendingServerId = serverId;
                }
                else
                {
                    this.ErrorMessage = errorMessage;
                    this.showErrorMessageWindow = true;
                    pendingServerId = "";
                }

            }
        }

        void DrawHostGameWindow(int windowID)
        {
            var rows = 4;
            var halfWindowWidth = windowInnerRect.width / 2;

            // Define a style for the title label
            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 18;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle labelSmallStyle = new(labelStyle);
            labelSmallStyle.alignment = TextAnchor.UpperLeft;
            labelSmallStyle.fontSize = 12;


            // Define a style for buttons
            GUIStyle buttonStyle = new(GUI.skin.button);
            buttonStyle.fontSize = 30;
            buttonStyle.fontStyle = FontStyle.Bold;

            for (var iRow = 0; iRow < rows; iRow++)
            {
                var y = 20 + (iRow * 60);

                switch (iRow)
                {
                    case 0:
                        // Title label for the number of players
                        GUI.Label(new UnityEngine.Rect(10, y, windowInnerRect.width - 20, 30), StayInTarkovPlugin.LanguageDictionary["NUMBER_OF_PLAYERS_TO_WAIT_FOR_MESSAGE"], labelStyle);
                        break;

                    case 1:
                        // Decrease button
                        if (GUI.Button(new UnityEngine.Rect(halfWindowWidth - 50, y, 30, 30), "-", buttonStyle))
                        {
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers > 1)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers -= 1;
                            }
                        }

                        // Player count label
                        GUI.Label(new UnityEngine.Rect(halfWindowWidth - 15, y, 30, 30), MatchmakerAcceptPatches.HostExpectedNumberOfPlayers.ToString(), labelStyle);

                        // Increase button
                        if (GUI.Button(new UnityEngine.Rect(halfWindowWidth + 20, y, 30, 30), "+", buttonStyle))
                        {
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers < 10)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers += 1;
                            }
                        }
                        break;

                    case 2:
                        CalculateXAxis PasswordAmountXAxis = new(new GUIContent(StayInTarkovPlugin.LanguageDictionary["REQUIRE_PASSWORD"]), halfWindowWidth);

                        // Checkbox to toggle the password field visibility
                        showPasswordField = GUI.Toggle(new UnityEngine.Rect(PasswordAmountXAxis.Checkbox, y, 200, 30), showPasswordField, "");

                        // "Require Password" text
                        GUI.Label(new UnityEngine.Rect(PasswordAmountXAxis.CheckboxText, y, PasswordAmountXAxis.Text, 30), StayInTarkovPlugin.LanguageDictionary["REQUIRE_PASSWORD"], labelSmallStyle);

                        // Password field (visible only when the checkbox is checked)
                        var passwordFieldWidth = 200;
                        var passwordFieldX = halfWindowWidth - passwordFieldWidth / 2;

                        if (showPasswordField)
                        {
                            passwordInput = GUI.PasswordField(new UnityEngine.Rect(passwordFieldX, y + 30, 200, 30), passwordInput, '*', 25);
                        }

                        break;

                    case 3:
                        var botSettingtFieldWidth = 350;
                        var botSettingsX = halfWindowWidth - botSettingtFieldWidth / 1.5f;

                        //Ai Amount
                        CalculateXAxis BotAmountXAxis = new(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_AMOUNT"]), halfWindowWidth);
                        GUI.Label(new Rect(BotAmountXAxis.Text, y, GUI.skin.label.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_AMOUNT"])).x, 60), StayInTarkovPlugin.LanguageDictionary["AI_AMOUNT"], labelSmallStyle);
                        Rect botAmountGridRect = new Rect(botSettingsX, y + 20, BotAmountStringOptions.Count() * 80, 30);
                        botAmountInput = GUI.SelectionGrid(botAmountGridRect, botAmountInput, BotAmountStringOptions, 6);

                        //Ai Difficulty
                        CalculateXAxis BotDifficultyXAxis = new(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_DIFFICULTY"]), halfWindowWidth);
                        GUI.Label(new Rect(BotDifficultyXAxis.Text, y + 55, GUI.skin.label.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_DIFFICULTY"])).x, 60), StayInTarkovPlugin.LanguageDictionary["AI_DIFFICULTY"], labelSmallStyle);
                        Rect botDifficultyGridRect = new Rect(botSettingsX, y + 80, BotDifficultyStringOptions.Count() * 80, 30);
                        botDifficultyInput = GUI.SelectionGrid(botDifficultyGridRect, botDifficultyInput, BotDifficultyStringOptions, 6);

                        //Bosses enabled - disabled
                        CalculateXAxis BotBossesEnabledXaxis = new(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_BOSSES_ENABLED"]), halfWindowWidth);
                        BotBossesEnabled = GUI.Toggle(new Rect(BotBossesEnabledXaxis.Checkbox, y + 115, 200, 25), BotBossesEnabled, "");
                        GUI.Label(new Rect(BotBossesEnabledXaxis.CheckboxText, y + 115, GUI.skin.label.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_BOSSES_ENABLED"])).x, 60), StayInTarkovPlugin.LanguageDictionary["AI_BOSSES_ENABLED"], labelSmallStyle);

                        break;
                }
            }

            // Style for back and start button
            GUIStyle smallButtonStyle = new(GUI.skin.button);
            smallButtonStyle.fontSize = 18;
            smallButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Back button
            if (GUI.Button(new UnityEngine.Rect(10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), StayInTarkovPlugin.LanguageDictionary["BACK"], smallButtonStyle))
            {
                showHostGameWindow = false;
                showServerBrowserWindow = true;
            }

            // Start button
            if (GUI.Button(new UnityEngine.Rect(halfWindowWidth + 10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), StayInTarkovPlugin.LanguageDictionary["START"], smallButtonStyle))
            {
                HostRaidAndJoin();
            }
        }

        private void HostRaidAndJoin()
        {
            FixesHideoutMusclePain();
            RaidSettings.BotSettings.BotAmount = (EBotAmount)botAmountInput;
            RaidSettings.WavesSettings.BotAmount = (EBotAmount)botAmountInput;

            RaidSettings.WavesSettings.BotDifficulty = (EBotDifficulty)botDifficultyInput;

            RaidSettings.WavesSettings.IsBosses = BotBossesEnabled;

            MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings, passwordInput);
            OriginalAcceptButton.OnClick.Invoke();


            JObject joinPacket = new();
            joinPacket.Add("profileId", MatchmakerAcceptPatches.Profile.ProfileId);
            joinPacket.Add("serverId", MatchmakerAcceptPatches.Profile.ProfileId);
            joinPacket.Add("m", "JoinMatch");
            AkiBackendCommunication.Instance.PostDownWebSocketImmediately(joinPacket.SITToJson());

            DestroyThis();
        }

        void FixesHideoutMusclePain()
        {
            // Check if hideout world exists
            var world = Comfort.Common.Singleton<GameWorld>.Instance;
            if (world == null)
                return;

            foreach (EFT.Player player in world.RegisteredPlayers)
            {
                // There should be 1 player only, but well, who knows if some bugs remain...
                if (player.IsYourPlayer)
                {
                    HealthController.MusclePain musclePain = player.HealthController.FindActiveEffect<HealthController.MusclePain>(EBodyPart.Common);
                    if (musclePain != null)
                    {
                        musclePain.Remove();
                    }
                    HealthController.SevereMusclePain severeMusclePain = player.HealthController.FindActiveEffect<HealthController.SevereMusclePain>(EBodyPart.Common);
                    if (severeMusclePain != null)
                    {
                        severeMusclePain.Remove();
                    }
                    break;
                }
            }
        }

        void DestroyThis()
        {
            StopAllTasks = true;

            GameObject.DestroyImmediate(this.gameObject);
            GameObject.DestroyImmediate(this);
        }
    }

    internal class CalculateXAxis
    {
        /// <summary>
        /// X-Axis for a checkbox
        /// </summary>
        public float Checkbox { get; private set; } = 0;
        /// <summary>
        /// X-Axis to put text besides a checkbox
        /// </summary>
        public float CheckboxText { get; private set; } = 0;
        /// <summary>
        /// X-Axis for text that's not needed to be formatted next to a checkbox
        /// </summary>
        public float Text { get; private set; } = 0;

        public CalculateXAxis(GUIContent Content, float WindowWidth, int HorizontalSpacing = 10)
        {
            var CalcSize = GUI.skin.label.CalcSize(Content);

            Checkbox = WindowWidth - CalcSize.x / 2 - HorizontalSpacing;
            CheckboxText = Checkbox + 20;
            Text = Checkbox;
        }
    }
}
