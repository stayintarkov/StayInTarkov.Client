using BepInEx.Logging;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib.Tools;
using Newtonsoft.Json.Linq;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Networking;
using StayInTarkov.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Systems.Effects;
using UnityEngine;
using UnityEngine.Yoga;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;

namespace StayInTarkov.Coop.Components
{
    /// <summary>
    /// This is the "Server Browser" and replacement of the "last screen"
    /// </summary>
    internal class SITMatchmakerGUIComponent : MonoBehaviour
    {
        private UnityEngine.Rect windowRect = new(20, 20, 120, 50);
        private UnityEngine.Rect windowInnerRect { get; set; } = new(20, 20, 120, 50);
        private GUIStyle styleBrowserRaidLabel { get; } = new GUIStyle();
        private GUIStyle styleBrowserRaidRow { get; } = new GUIStyle() { };
        private GUIStyle styleBrowserRaidLink { get; } = new GUIStyle();

        private GUIStyle styleBrowserWindow { get; set; }

        private GUIStyleState styleStateBrowserBigButtonsNormal;
        private GUIStyle styleBrowserBigButtons { get; set; }

        public RaidSettings RaidSettings { get; internal set; }
        public DefaultUIButton OriginalBackButton { get; internal set; }
        public DefaultUIButton OriginalAcceptButton { get; internal set; }

        private Task GetMatchesTask { get; set; }

        private JArray m_Matches { get; set; }

        private CancellationTokenSource m_cancellationTokenSource;

        private bool StopAllTasks = false;

        private bool showPasswordField = false;

        private string passwordInput = "";
        private string passwordClientInput = "";

        private int botAmountInput = 0;
        private int botDifficultyInput = 0;
        private int protocolInput = 1;
        private bool p2pModeSelected => protocolInput == 1;

        private string pendingServerId = "";
        private int p2pModeOptionInput;

        private string[] BotAmountStringOptions = new string[]
        {
         (string) StayInTarkovPlugin.LanguageDictionary["SAME_AS_ONLINE"],
         (string) StayInTarkovPlugin.LanguageDictionary["NONE_AI"],
         (string) StayInTarkovPlugin.LanguageDictionary["LOW"],
         (string) StayInTarkovPlugin.LanguageDictionary["MEDIUM"],
         (string) StayInTarkovPlugin.LanguageDictionary["HIGH"],
         (string) StayInTarkovPlugin.LanguageDictionary["HORDE"]
        };

        private string[] BotDifficultyStringOptions = new string[]
        {
         (string) StayInTarkovPlugin.LanguageDictionary["SAME_AS_ONLINE_DIFFICULTY"],
         (string) StayInTarkovPlugin.LanguageDictionary["EASY_DIFFICULTY"],
         (string) StayInTarkovPlugin.LanguageDictionary["MEDIUM_DIFFICULTY"],
         (string) StayInTarkovPlugin.LanguageDictionary["HARD_DIFFICULTY"],
         (string) StayInTarkovPlugin.LanguageDictionary["IMPOSSIBLE_DIFFICULTY"],
         (string) StayInTarkovPlugin.LanguageDictionary["RANDOM_DIFFICULTY"]
        };

        private string[] ProtocolStringOptions = StayInTarkovPlugin.LanguageDictionary["PROTOCOL_OPTIONS"].ToArray().Select(x=>x.ToString()).ToArray();
        private string[] P2PModeStringOptions = StayInTarkovPlugin.LanguageDictionary["P2P_MODE_OPTIONS"].ToArray().Select(x=>x.ToString()).ToArray();

        private bool BotBossesEnabled = true;

        private const float verticalSpacing = 10f;

        private ManualLogSource Logger { get; set; }
        public MatchMakerPlayerPreview MatchMakerPlayerPreview { get; internal set; }

        //public Canvas Canvas { get; set; }
        public Profile Profile { get; internal set; }
        public Rect hostGameWindowInnerRect { get; private set; }

        public bool IsMatchmakingAvailable { get; private set; } = false;

        public bool IsHeadlessServerAvailable { get; private set; } = false;

        #region TextMeshPro Game Objects 

        private GameObject GOIPv4_Text { get; set; }    
        private GameObject GOIPv6_Text { get; set; }

        #endregion

        #region Window Determination

        private bool showHostGameWindow { get; set; }
        private bool showServerBrowserWindow { get; set; } = true;
        private bool showErrorMessageWindow { get; set; } = false;
        private bool showPasswordRequiredWindow { get; set; } = false;



        #endregion

        #region Unity

        void Start()
        {
            // Setup Logger
            Logger = BepInEx.Logging.Logger.CreateLogSource("SIT Matchmaker GUI");
            Logger.LogInfo("Start");

            TMPManager = new PaulovTMPManager();

            styleStateBrowserBigButtonsNormal = new GUIStyleState()
            {
                textColor = Color.white
            };

            // Create background Texture
            Texture2D texture2D = new(128, 128);
            texture2D.Fill(Color.black);
            styleStateBrowserBigButtonsNormal.background = texture2D;
            styleStateBrowserBigButtonsNormal.textColor = Color.black;

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

            SITRearrangeScreen();

            DeleteExistingMatches();
        }

        private GameObject SITServersAvailable { get; set; }
        private GameObject MatchmakingServersAvailable { get; set; }

        private void SITRearrangeScreen()
        {
            var previewsPanel = GameObject.Find("PreviewsPanel");
            if (previewsPanel != null)
            {
                var previewsPanelRect = previewsPanel.GetComponent<RectTransform>();
                previewsPanelRect.position = new Vector3(400, 290, 0);

                var levelPanel = GameObject.Find("Level Panel");
                if (levelPanel != null)
                {
                    var levelPanelRect = levelPanel.GetComponent<RectTransform>();
                    levelPanelRect.position = new Vector3(150, Screen.height, 0);
                }
            }

            var playerImage = GameObject.Find("PlayerImage");
            if (playerImage != null)
            {
                var playerImageRect = playerImage.GetComponent<RectTransform>();
                playerImageRect.localScale = new Vector3(1.39f, 1.39f, 0);
            }

            var playerName = GameObject.Find("PlayerInfo");
            if (playerName != null)
            {
                var playerNameRect = playerName.GetComponent<RectTransform>();
                playerNameRect.position = new Vector3(Screen.width * 0.2f, Screen.height - (Screen.height * 0.02f), 0);
                playerNameRect.localScale = new Vector3(1.35f, 1.35f, 0);

            }

            ScreenCaption = GameObject.Find("Screen Caption");
            if (ScreenCaption != null)
            {
                Logger.LogInfo("Found Screen Caption");
                ScreenCaption.gameObject.SetActive(false);
            }

            Subcaption = GameObject.Find("Subcaption");
            if (ScreenCaption != null)
            {
                Logger.LogInfo("Found Subcaption");
                Subcaption.gameObject.SetActive(false);
            }

            SITServersAvailable = this.TMPManager.InstantiateTarkovTextLabel(nameof(SITServersAvailable), "", 20, new Vector3(((Screen.height / 2)) - 100, ((Screen.height / 2)) - 60, 0));
        }

        private void DeleteExistingMatches()
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("serverId", AkiBackendCommunication.Instance.ProfileId);
            AkiBackendCommunication.Instance.PostJsonBLOCKING("/coop/server/delete", jsonObj.ToString());
        }

      
        void OnDestroy()
        {
            if (m_cancellationTokenSource != null)
                m_cancellationTokenSource.Cancel();

            StopAllTasks = true;
        }

        GameObject ScreenCaption { get; set; }
        GameObject Subcaption { get; set; }

        void Update()
        {
            if(this.m_Matches != null)
            {
                var countOfAvailableServers =
                   // Count open servers that are Waiting for Players
                   this.m_Matches
                   .Count(match => match["Status"]?.ToString() == SITMPRaidStatus.WaitingForPlayers.ToString());

                var serverAvailabilityText = $"{countOfAvailableServers} Servers Available " 
                    + $"{(this.IsMatchmakingAvailable ? " | Matchmaking Available" : "")}"
                    + $"{(this.IsHeadlessServerAvailable ? " | Headless Server Available" : "")}";

                this.SITServersAvailable.GetComponent<CustomTextMeshProUGUI>().text = serverAvailabilityText;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DestroyThis();
            }

        }

        void OnGUI()
        {
            // Draw the horizontal line for top bar
            GUI.DrawTexture(new Rect(0, Screen.height * 0.08f, Screen.width, 2), Texture2D.grayTexture);

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
                if (GUI.Button(new UnityEngine.Rect(buttonX, buttonY, buttonWidth, buttonHeight), StayInTarkovPlugin.LanguageDictionary["HOST_RAID"].ToString(), gamemodeButtonStyle))
                {
                    showServerBrowserWindow = false;
                    showHostGameWindow = true;
                }

                // Create "Play Single Player" button next to the "Host Game" button
                buttonX += buttonWidth + 10;
                if (GUI.Button(new UnityEngine.Rect(buttonX, buttonY, buttonWidth, buttonHeight), StayInTarkovPlugin.LanguageDictionary["PLAY_SINGLE_PLAYER"].ToString(), gamemodeButtonStyle))
                {
                    Logger.LogDebug("Click Play Single Player Button");
                    FixesHideoutMusclePain();
                    //OriginalAcceptButton.OnClick.Invoke();
                    HostSoloRaidAndJoin();
                }
            }
            else if (showHostGameWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawHostGameWindow, StayInTarkovPlugin.LanguageDictionary["HOST_RAID"].ToString());
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

                if (GUI.Button(new UnityEngine.Rect(backButtonX, backButtonY, 200, 40), StayInTarkovPlugin.LanguageDictionary["BACK"].ToString(), buttonStyle))
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
                    var result = await AkiBackendCommunication.Instance.PostJsonAsync("/coop/server/getAllForLocation", RaidSettings.ToJson(), timeout: 4000);
                    if (result != null)
                    {
                        m_Matches = JArray.Parse(result);
                    }

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(5000);

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
        public PaulovTMPManager TMPManager { get; private set; }

        /// <summary>
        /// TODO: Finish this on Error Window
        /// </summary>
        /// <param name="windowID"></param>
        void DrawWindowErrorMessage(int windowID)
        {
            if (!showErrorMessageWindow)
                return;

            GUI.Label(new UnityEngine.Rect(20, 20, 200, 200), ErrorMessage);

            if (GUI.Button(new UnityEngine.Rect(20, windowInnerRect.height - 90, windowInnerRect.width - 40, 45), StayInTarkovPlugin.LanguageDictionary["CLOSE"].ToString()))
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

            var PasswordTextWidth = GUI.skin.label.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["ENTER-PASSWORD"].ToString())).x;

            var textX = halfWindowWidth - PasswordTextWidth / 2;

            GUI.Label(new UnityEngine.Rect(textX, halfWindowHeight - 100, PasswordTextWidth, 30), StayInTarkovPlugin.LanguageDictionary["ENTER-PASSWORD"].ToString());

            var passwordFieldWidth = 200;
            var passwordFieldX = halfWindowWidth - passwordFieldWidth / 2;

            passwordClientInput = GUI.PasswordField(new UnityEngine.Rect(passwordFieldX, halfWindowHeight - 50, 200, 30), passwordClientInput, '*', 25);

            var buttonX = halfWindowWidth - PasswordTextWidth / 2;

            if (GUI.Button(new UnityEngine.Rect(buttonX - 60, halfWindowHeight, 100, 40), StayInTarkovPlugin.LanguageDictionary["BACK"].ToString()))
            {
                showPasswordRequiredWindow = false;
                showServerBrowserWindow = true;
            }

            if (GUI.Button(new UnityEngine.Rect(buttonX + 60, halfWindowHeight, 100, 40), StayInTarkovPlugin.LanguageDictionary["JOIN"].ToString()))
            {
                JoinMatch(SITMatchmaking.Profile.ProfileId, pendingServerId, passwordClientInput);
            }
        }

        void DrawBrowserWindow(int windowID)
        {
            // Define column labels
            // Use the Language Dictionary
            string[] columnLabels = {
                StayInTarkovPlugin.LanguageDictionary["SERVER"].ToString()
                , StayInTarkovPlugin.LanguageDictionary["LOCATION"].ToString()
                , StayInTarkovPlugin.LanguageDictionary["PASSWORD"].ToString()
            };


            // Define the button style
            GUIStyle buttonStyle = new(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(6, 6, 6, 6);
            //Font myFont = (Font)Resources.Load("Fonts/comic", typeof(Font));

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

            if (m_Matches == null)
                return;

            //Logger.LogDebug(m_Matches.ToString());

            var index = 0;
            var yPosOffset = 60;

            foreach (var match in m_Matches)
            {
                var yPos = yPosOffset + index * (cellHeight + 5);

                string protocol = match["Protocol"]?.ToString();
                string status = match["Status"]?.ToString();

                // Display Host Name with "Raid" label
                GUI.Label(new UnityEngine.Rect(10, yPos, cellWidth - separatorWidth, cellHeight), $"{match["HostName"]} Raid", labelStyle);

                // Display Location
                GUI.Label(new UnityEngine.Rect(cellWidth * 1, yPos, cellWidth - separatorWidth, cellHeight), match["Location"].ToString(), labelStyle);

                // Display Password Locked
                GUI.Label(new UnityEngine.Rect(cellWidth * 2, yPos, cellWidth - separatorWidth, cellHeight), bool.Parse(match["IsPasswordLocked"].ToString()) ? (string)StayInTarkovPlugin.LanguageDictionary["PASSWORD-YES"] : "", labelStyle);

                // Calculate the width of the combined server information (Host Name, Player Count, Location)
                var serverInfoWidth = cellWidth * 3 - separatorWidth * 2;

                if (status == "WaitingForPlayers")
                {
                    // Create "Join" button for each match on the next column
                    if (GUI.Button(new UnityEngine.Rect(cellWidth * 3 + separatorWidth / 2 + 15, yPos + (cellHeight * 0.3f), cellWidth * 0.8f, cellHeight * 0.5f), StayInTarkovPlugin.LanguageDictionary["JOIN"].ToString(), buttonStyle))
                    {
                        // Perform actions when the "Join" button is clicked
                        JoinMatch(SITMatchmaking.Profile.ProfileId, match["ServerId"].ToString());
                    }
                }

                index++;
            }
        }

        void JoinMatch(string profileId, string serverId, string password = "")
        {
            if (SITMatchmaking.TryJoinMatch(RaidSettings, profileId, serverId, password, out string returnedJson, out string errorMessage))
            {
                Logger.LogDebug(returnedJson);
                JObject result = JObject.Parse(returnedJson);

                Enum.TryParse(result["protocol"].ToString(), out ESITProtocol protocol);
                Logger.LogDebug($"{nameof(SITMatchmakerGUIComponent)}:{nameof(JoinMatch)}:{protocol}");
                SITMatchmaking.SITProtocol = protocol;
                SITMatchmaking.SetGroupId(result["serverId"].ToString());
                SITMatchmaking.SetTimestamp(long.Parse(result["timestamp"].ToString()));
                SITMatchmaking.HostExpectedNumberOfPlayers = int.Parse(result["expectedNumberOfPlayers"].ToString());

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

        //GameObject GameObject_NumberOfPlayersToWaitFor;

        void DrawHostGameWindow(int windowID)
        {
            var rows = 10;
            var halfWindowWidth = windowInnerRect.width / 2;

            var cols = new float[] { halfWindowWidth * 0.1f, halfWindowWidth * 0.66f, halfWindowWidth * 1.01f, halfWindowWidth * 1.33f };

            // Define a style for the title label
            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle infoStyle = new(labelStyle);
            infoStyle.alignment = TextAnchor.MiddleLeft;
            infoStyle.fontStyle = FontStyle.Bold;
            infoStyle.fontSize = 11;

            // Define a style for buttons
            GUIStyle buttonStyle = new(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Normal;

            var buttonSize = 25;

            var lblNumberOfPlayers = StayInTarkovPlugin.LanguageDictionary["NUMBER_OF_PLAYERS_TO_WAIT_FOR_MESSAGE"].ToString();
            var contentLabelNumberOfPlayers = new GUIContent(lblNumberOfPlayers);
            var calcSizeContentLabelNumberOfPlayers = labelStyle.CalcSize(contentLabelNumberOfPlayers);

            for (var iRow = 0; iRow < rows; iRow++)
            {
                var y = 25 + (iRow * 35);

                switch (iRow)
                {
                    case 0:

                        // Title label for the number of players
                        GUI.Label(new Rect(cols[0], y, calcSizeContentLabelNumberOfPlayers.x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["NUMBER_OF_PLAYERS_TO_WAIT_FOR_MESSAGE"].ToString(), labelStyle);

                        // Decrease button
                        if (GUI.Button(new Rect(cols[1], y, buttonSize, buttonSize), "-", buttonStyle))
                        {
                            if (SITMatchmaking.HostExpectedNumberOfPlayers > 1)
                            {
                                SITMatchmaking.HostExpectedNumberOfPlayers -= 1;
                            }
                        }

                        var contentNumberOfPlayers = new GUIContent(SITMatchmaking.HostExpectedNumberOfPlayers.ToString());
                        // Player count label
                        GUI.Label(new Rect(cols[2], y, labelStyle.CalcSize(contentNumberOfPlayers).x, labelStyle.CalcSize(contentNumberOfPlayers).y), SITMatchmaking.HostExpectedNumberOfPlayers.ToString(), labelStyle);

                        // Increase button
                        if (GUI.Button(new Rect(cols[3], y, buttonSize, buttonSize), "+", buttonStyle))
                        {
                            if (SITMatchmaking.HostExpectedNumberOfPlayers < 99)
                            {
                                SITMatchmaking.HostExpectedNumberOfPlayers += 1;
                            }
                        }
                        break;

                    case 1:
                        // "Require Password" text
                        GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["REQUIRE_PASSWORD"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["REQUIRE_PASSWORD"].ToString(), labelStyle);

                        // Checkbox to toggle the password field visibility
                        showPasswordField = GUI.Toggle(new Rect(cols[1], y, buttonSize, buttonSize), showPasswordField, "");
                     
                        // Password field (visible only when the checkbox is checked)
                        var passwordFieldWidth = 200;
                        var passwordFieldX = cols[1] + buttonSize + 10;

                        if (showPasswordField)
                        {
                            passwordInput = GUI.PasswordField(new Rect(passwordFieldX, y, passwordFieldWidth, buttonSize), passwordInput, '*', 25);
                        }

                        break;

                    case 2:
                        //Ai Amount
                        GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_AMOUNT"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["AI_AMOUNT"].ToString(), labelStyle);
                        Rect botAmountGridRect = new Rect(cols[1], y, BotAmountStringOptions.Count() * 80, 25);
                        botAmountInput = GUI.SelectionGrid(botAmountGridRect, botAmountInput, BotAmountStringOptions, 6);
                       
                        break;
                    case 3:

                        //Ai Difficulty
                        GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_DIFFICULTY"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["AI_DIFFICULTY"].ToString(), labelStyle);
                        Rect botDifficultyGridRect = new Rect(cols[1], y, BotDifficultyStringOptions.Count() * 80, 25);
                        botDifficultyInput = GUI.SelectionGrid(botDifficultyGridRect, botDifficultyInput, BotDifficultyStringOptions, 6);

                        break;
                    case 4:
                        //Bosses enabled - disabled
                        GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["AI_BOSSES_ENABLED"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["AI_BOSSES_ENABLED"].ToString(), labelStyle);
                        BotBossesEnabled = GUI.Toggle(new Rect(cols[1], y, 200, 25), BotBossesEnabled, "");

                        break;
                    case 5:
                        // Protocol Choice
                        GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["PROTOCOL"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["PROTOCOL"].ToString(), labelStyle);

                        Rect protocolGridRect = new Rect(cols[1], y, ProtocolStringOptions.Count() * 120, 25);
                        protocolInput = GUI.SelectionGrid(protocolGridRect, protocolInput, ProtocolStringOptions, ProtocolStringOptions.Count());

                        break;
                    case 6:
                        // If Peer to Peer is chosen
                        if (this.p2pModeSelected)
                        {
                            // P2P Mode Option Choice
                            GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["P2P_MODE_LABEL"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["P2P_MODE_LABEL"].ToString(), labelStyle);

                            Rect p2pModeOptionGridRect = new Rect(cols[1], y, P2PModeStringOptions.Count() * 120, 25);
                            p2pModeOptionInput = GUI.SelectionGrid(p2pModeOptionGridRect, p2pModeOptionInput, P2PModeStringOptions, P2PModeStringOptions.Count());
                        }
                        break;
                    case 7:
                        // If Peer to Peer is chosen
                        if (this.p2pModeSelected)
                        {
                            // Automatic
                            if (p2pModeOptionInput == 0)
                            {
                                GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["P2P_AUTOMATIC_INFO_LABEL"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["P2P_AUTOMATIC_INFO_LABEL"].ToString(), infoStyle);
                            }
                            // Manual
                            if (p2pModeOptionInput == 1)
                            {
                                GUI.Label(new Rect(cols[0], y, labelStyle.CalcSize(new GUIContent(StayInTarkovPlugin.LanguageDictionary["P2P_MANUAL_INFO_LABEL"].ToString())).x, calcSizeContentLabelNumberOfPlayers.y), StayInTarkovPlugin.LanguageDictionary["P2P_MANUAL_INFO_LABEL"].ToString(), infoStyle);
                            }
                        }
                        break;
                }
            }

            // Style for back and start button
            GUIStyle smallButtonStyle = new(GUI.skin.button);
            smallButtonStyle.fontSize = 18;
            smallButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Back button
            if (GUI.Button(new Rect(10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), StayInTarkovPlugin.LanguageDictionary["BACK"].ToString(), smallButtonStyle))
            {
                Logger.LogDebug("Click Back Button");
                showHostGameWindow = false;
                showServerBrowserWindow = true;
            }

            // Start button
            if (GUI.Button(new Rect(halfWindowWidth + 10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), StayInTarkovPlugin.LanguageDictionary["START"].ToString(), smallButtonStyle))
            {
                Logger.LogDebug("Click Start Button");
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

            var addr = "";
            var port = 0;

            // use config values in P2P manual mode
            if (((ESITProtocol)protocolInput) == ESITProtocol.PeerToPeerUdp && p2pModeOptionInput == 1)
            {
                addr = PluginConfigSettings.Instance.CoopSettings.UdpServerPublicIP;
                port = PluginConfigSettings.Instance.CoopSettings.UdpServerPublicPort;
            }

            SITMatchmaking.CreateMatch(
                SITMatchmaking.Profile.ProfileId
                , RaidSettings
                , passwordInput
                , (ESITProtocol)protocolInput
                , addr
                , port
                , EMatchmakerType.GroupLeader);
            OriginalAcceptButton.OnClick.Invoke();

            JObject joinPacket = new();
            joinPacket.Add("profileId", SITMatchmaking.Profile.ProfileId);
            joinPacket.Add("serverId", SITMatchmaking.Profile.ProfileId);
            joinPacket.Add("m", "JoinMatch");
            AkiBackendCommunication.Instance.PostDownWebSocketImmediately(joinPacket.SITToJson());

            DestroyThis();
        }

        public void HostSoloRaidAndJoin(ESITProtocol protocol = ESITProtocol.RelayTcp, EBotAmount botAmount = EBotAmount.AsOnline)
        {
            FixesHideoutMusclePain();

            RaidSettings.BotSettings.BotAmount = botAmount;
            RaidSettings.WavesSettings.BotAmount = botAmount;
            RaidSettings.WavesSettings.BotDifficulty = EBotDifficulty.AsOnline;
            RaidSettings.WavesSettings.IsBosses = true;

            SITMatchmaking.HostExpectedNumberOfPlayers = 1;

            SITMatchmaking.CreateMatch(
                SITMatchmaking.Profile.ProfileId
                , RaidSettings
                , ""
                , protocol
                , null
                , PluginConfigSettings.Instance.CoopSettings.UdpServerLocalPort,
                EMatchmakerType.GroupLeader);
            OriginalAcceptButton.OnClick.Invoke();

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
                    HealthControllerClass.MusclePain musclePain = player.HealthController.FindActiveEffect<HealthControllerClass.MusclePain>(EBodyPart.Common);
                    if (musclePain != null)
                    {
                        musclePain.Remove();
                    }
                    //HealthControllerClass.SevereMusclePain severeMusclePain = player.HealthController.FindActiveEffect<HealthControllerClass.SevereMusclePain>(EBodyPart.Common);
                    //if (severeMusclePain != null)
                    //{
                    //    severeMusclePain.Remove();
                    //}
                    break;
                }
            }
        }

        void DestroyThis()
        {
            StopAllTasks = true;

            if (this.TMPManager != null)
                TMPManager.DestroyObjects();

            if (this.gameObject != null)
                GameObject.DestroyImmediate(this.gameObject);

            GameObject.DestroyImmediate(this);
        }
    }
}
