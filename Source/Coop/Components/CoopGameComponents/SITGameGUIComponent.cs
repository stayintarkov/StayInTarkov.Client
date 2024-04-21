using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Networking;
using StayInTarkov.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StayInTarkov.Coop.Components.CoopGameComponents.SITGameComponent;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    public class SITGameGUIComponent : MonoBehaviour
    {

        GUIStyle middleLabelStyle;
        GUIStyle middleLargeLabelStyle;
        GUIStyle normalLabelStyle;

        private SITGameComponent CoopGameComponent { get { return SITGameComponent.GetCoopGameComponent(); } }

        private ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent?.Players;

        private IEnumerable<EFT.Player> Users => CoopGameComponent?.PlayerUsers;

        private EFT.Player[] Bots => CoopGameComponent?.PlayerBots;

        private SITConfig SITConfig => CoopGameComponent?.SITConfig;

        float screenScale = 1.0f;
        private ManualLogSource Logger;

        Camera GameCamera { get; set; }

        int GuiX = 10;
        int GuiWidth = 400;

        private PaulovTMPManager TMPManager { get; } = new PaulovTMPManager();
        private GameObject _endGameMessageGO;

        void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($"{nameof(SITGameGUIComponent)}");
            Logger.LogDebug($"{nameof(SITGameGUIComponent)}:Awake");

            _endGameMessageGO = TMPManager.InstantiateTarkovTextLabel("_EndGameMessage", "", 20, new Vector3(0, (Screen.height / 2) - 120, 0));
        }

        void OnGUI()
        {
            if (normalLabelStyle == null)
            {
                normalLabelStyle = new GUIStyle(GUI.skin.label);
                normalLabelStyle.fontSize = 16;
                normalLabelStyle.fontStyle = FontStyle.Bold;
            }
            if (middleLabelStyle == null)
            {
                middleLabelStyle = new GUIStyle(GUI.skin.label);
                middleLabelStyle.fontSize = 18;
                middleLabelStyle.fontStyle = FontStyle.Bold;
                middleLabelStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (middleLargeLabelStyle == null)
            {
                middleLargeLabelStyle = new GUIStyle(middleLabelStyle);
                middleLargeLabelStyle.fontSize = 24;
            }

            var rect = new Rect(GuiX, 0, GuiWidth, 100);
            rect = DrawPing(rect);

            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 13;

            var w = 0.5f; // proportional width (0..1)
            var h = 0.2f; // proportional height (0..1)
            var rectEndOfGameMessage = Rect.zero;
            rectEndOfGameMessage.x = (float)(Screen.width * (1 - w)) / 2;
            rectEndOfGameMessage.y = (float)(Screen.height * (1 - h)) / 2 + Screen.height / 3;
            rectEndOfGameMessage.width = Screen.width * w;
            rectEndOfGameMessage.height = Screen.height * h;

            var numberOfPlayersDead = Users.Count(x => !x.HealthController.IsAlive);

            var quitState = CoopGameComponent.GetQuitState();
            switch (quitState)
            {
                case EQuitState.YourTeamIsDead:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_DEAD"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YouAreDead:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_SOLO"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YouAreDeadAsHost:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_HOST"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YouAreDeadAsClient:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_DEAD_CLIENT"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YourTeamHasExtracted:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_TEAM_EXTRACTED"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YouHaveExtractedOnlyAsHost:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED_HOST"].ToString(), middleLargeLabelStyle);
                    break;
                case EQuitState.YouHaveExtractedOnlyAsClient:
                    GUI.Label(rectEndOfGameMessage, StayInTarkovPlugin.LanguageDictionary["RAID_PLAYER_EXTRACTED_CLIENT"].ToString(), middleLargeLabelStyle);
                    break;
            }

            OnGUI_DrawPlayerFriendlyTags(rect);
            OnGUI_DrawPlayerEnemyTags(rect);

        }

        private Rect DrawPing(Rect rect)
        {
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_ShowSITStatistics)
                return rect;

            var prevFontSz = GUI.skin.label.fontSize;
            var prevAlign = GUI.skin.label.alignment;

            var row = () => rect.y += GUI.skin.label.fontSize + 1;

            var gameclient = Singleton<ISITGame>.Instance.GameClient;

            rect.x += 65;

            GUI.skin.label.fontSize = 10;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.contentColor = Color.white;
            var protocol = SITMatchmaking.SITProtocol switch
            {
                ESITProtocol.RelayTcp => "tcp ",
                ESITProtocol.PeerToPeerUdp => "udp ",
                _ => "",
            };
            var text = new GUIContent($"{protocol}{(SITMatchmaking.IsClient ? "client" : "host")} ping:{gameclient.Ping} up:{gameclient.UploadSpeedKbps:0.00} down:{gameclient.DownloadSpeedKbps:0.00} loss:{gameclient.PacketLoss:0.00}% {(AkiBackendCommunication.Instance.HighPingMode ? "hpm" : "")}");
            GUI.Label(rect, text);
            GUI.contentColor = Color.white;

            row();
            rect.x -= 65;

            GUI.skin.label.fontSize = prevFontSz;
            GUI.skin.label.alignment = prevAlign;

            return rect;
        }

        private Rect DrawSITStats(Rect rect, int numberOfPlayersDead, CoopSITGame coopGame)
        {
            if (!PluginConfigSettings.Instance.CoopSettings.SETTING_ShowSITStatistics)
                return rect;

            var numberOfPlayersAlive = Users.Count(x => x.HealthController.IsAlive);
            // gathering extracted
            var numberOfPlayersExtracted = coopGame.ExtractedPlayers.Count;
            GUI.Label(rect, $"{StayInTarkovPlugin.LanguageDictionary["PLAYERS_ALIVE"]}: {numberOfPlayersAlive}");
            rect.y += 15;
            GUI.Label(rect, $"{StayInTarkovPlugin.LanguageDictionary["PLAYERS_DEAD"]}: {numberOfPlayersDead}");
            rect.y += 15;
            GUI.Label(rect, $"{StayInTarkovPlugin.LanguageDictionary["PLAYERS_EXTRACTED"]}: {numberOfPlayersExtracted}");
            rect.y += 15;
            GUI.Label(rect, $"{StayInTarkovPlugin.LanguageDictionary["BOTS"]}: {Bots.Length}");
            rect.y += 15;
            return rect;
        }

        private void OnGUI_DrawPlayerFriendlyTags(Rect rect)
        {
            try
            {
                if (SITConfig == null)
                {
                    Logger.LogError("SITConfig is null?");
                    return;
                }

                if (!SITConfig.showPlayerNameTags)
                {
                    return;
                }

                if (FPSCamera.Instance == null)
                    return;

                if (Players == null)
                    return;

                if (Users == null)
                    return;

                if (Camera.current == null)
                    return;

                if (!Singleton<GameWorld>.Instantiated)
                    return;

                if (FPSCamera.Instance.SSAA != null && FPSCamera.Instance.SSAA.isActiveAndEnabled)
                    screenScale = FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

                var ownPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                if (ownPlayer == null)
                    return;

                // Paulov: Do not show labels whilst in Inventory, its annoying!
                if (ownPlayer.IsInventoryOpened)
                    return;

                foreach (var pl in Users)
                {
                    if (pl == null)
                        continue;

                    if (pl.HealthController == null)
                        continue;

                    if (pl.IsYourPlayer && pl.HealthController.IsAlive)
                        continue;

                    if (pl.PlayerBones == null)
                        continue;

                    Vector3 aboveBotHeadPos = pl.PlayerBones.Pelvis.position + Vector3.up * (pl.HealthController.IsAlive ? 1.1f : 0.3f);
                    Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                    if (screenPos.z > 0)
                    {
                        rect.x = screenPos.x * screenScale - rect.width / 2;
                        rect.y = Screen.height - (screenPos.y + rect.height / 2) * screenScale;

                        GUIStyle labelStyle = middleLabelStyle;
                        labelStyle.fontSize = 14;
                        float labelOpacity = 1;
                        float distanceToCenter = Vector3.Distance(screenPos, new Vector3(Screen.width, Screen.height, 0) / 2);

                        if (distanceToCenter < 100)
                        {
                            labelOpacity = distanceToCenter / 100;
                        }

                        if (ownPlayer.HandsController.IsAiming)
                        {
                            labelOpacity *= 0.5f;
                        }

                        if (pl.HealthController.IsAlive)
                        {
                            var maxHealth = pl.HealthController.GetBodyPartHealth(EBodyPart.Common).Maximum;
                            var currentHealth = pl.HealthController.GetBodyPartHealth(EBodyPart.Common).Current / maxHealth;
                            labelStyle.normal.textColor = new Color(2.0f * (1 - currentHealth), 2.0f * currentHealth, 0, labelOpacity);
                        }
                        else
                        {
                            labelStyle.normal.textColor = new Color(255, 0, 0, labelOpacity);
                        }

                        var distanceFromCamera = Math.Round(Vector3.Distance(Camera.current.gameObject.transform.position, pl.Position));
                        GUI.Label(rect, $"{pl.Profile.Nickname} {distanceFromCamera}m", labelStyle);
                    }
                }
            }
            catch
            {

            }
        }

        private void OnGUI_DrawPlayerEnemyTags(Rect rect)
        {
            try
            {
                if (SITConfig == null)
                {
                    Logger.LogError("SITConfig is null?");
                    return;
                }

                if (!SITConfig.showPlayerNameTagsForEnemies)
                {
                    return;
                }

                if (FPSCamera.Instance == null)
                    return;

                if (Players == null)
                    return;

                if (Users == null)
                    return;

                if (Camera.current == null)
                    return;

                if (!Singleton<GameWorld>.Instantiated)
                    return;


                if (FPSCamera.Instance.SSAA != null && FPSCamera.Instance.SSAA.isActiveAndEnabled)
                    screenScale = FPSCamera.Instance.SSAA.GetOutputWidth() / (float)FPSCamera.Instance.SSAA.GetInputWidth();

                var ownPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                if (ownPlayer == null)
                    return;

                foreach (var pl in Bots)
                {
                    if (pl == null)
                        continue;

                    if (pl.HealthController == null)
                        continue;

                    if (!pl.HealthController.IsAlive)
                        continue;

                    Vector3 aboveBotHeadPos = pl.Position + Vector3.up * (pl.HealthController.IsAlive ? 1.5f : 0.5f);
                    Vector3 screenPos = Camera.current.WorldToScreenPoint(aboveBotHeadPos);
                    if (screenPos.z > 0)
                    {
                        rect.x = screenPos.x * screenScale - rect.width / 2;
                        rect.y = Screen.height - screenPos.y * screenScale - 15;

                        var distanceFromCamera = Math.Round(Vector3.Distance(Camera.current.gameObject.transform.position, pl.Position));
                        GUI.Label(rect, $"{pl.Profile.Nickname} {distanceFromCamera}m", middleLabelStyle);
                        rect.y += 15;
                        GUI.Label(rect, $"X", middleLabelStyle);
                    }
                }
            }
            catch
            {

            }
        }

    }
}