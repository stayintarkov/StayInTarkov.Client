using EFT;
using EFT.UI.Matchmaker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Configuration;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Matchmaker
{
    public enum EMatchmakerType
    {
        Single = 0,
        GroupPlayer = 1,
        GroupLeader = 2
    }

    [Serializable]
    public class ServerStatus
    {
        [JsonProperty("ip")]
        public string ip { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }

    public static class MatchmakerAcceptPatches
    {
        #region Fields/Properties
        public static EFT.UI.Matchmaker.MatchMakerAcceptScreen MatchMakerAcceptScreenInstance { get; set; }
        public static string ServerType { get; set; }
        public static int ServerPort { get; set; }
        public static Profile Profile { get; set; }
        public static EMatchmakerType MatchingType { get; set; } = EMatchmakerType.Single;
        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;
        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;
        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static int HostExpectedNumberOfPlayers { get; set; } = 1;
        private static string groupId;
        private static long timestamp;
        #endregion

        #region Static Fields

        public static object MatchmakerScreenController
        {
            get
            {
                var screenController = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(MatchMakerAcceptScreenInstance, "ScreenController", false);
                if (screenController != null)
                {
                    StayInTarkovHelperConstants.Logger.LogInfo("MatchmakerAcceptPatches.Found ScreenController Instance");

                    return screenController;

                }
                return null;
            }
        }

        public static GameObject EnvironmentUIRoot { get; internal set; }
        public static MatchmakerTimeHasCome.TimeHasComeScreenController TimeHasComeScreenController { get; internal set; }
        #endregion

        public static void Run()
        {
            new EnvironmentUIRootPatch().Enable();
            new MatchmakerAcceptScreenAwakePatch().Enable();
            new MatchmakerAcceptScreenShowPatch().Enable();
        }

        public static string GetGroupId()
        {
            return groupId;
        }

        public static void SetGroupId(string newId)
        {
            groupId = newId;
        }

        public static long GetTimestamp()
        {
            return timestamp;
        }

        public static void SetTimestamp(long ts)
        {
            timestamp = ts;
        }

        public static bool CheckForMatch(RaidSettings settings, string password, out string outJson, out string errorMessage)
        {
            errorMessage = $"No server matches the data provided or the server no longer exists";
            StayInTarkovHelperConstants.Logger.LogInfo("CheckForMatch");
            outJson = string.Empty;

            if (MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance != null)
            {
                JObject settingsJSON = JObject.FromObject(settings);
                settingsJSON.Add("password", password);

                outJson = AkiBackendCommunication.Instance.PostJson("/coop/server/exist", JsonConvert.SerializeObject(settingsJSON));
                StayInTarkovHelperConstants.Logger.LogInfo(outJson);

                if (!string.IsNullOrEmpty(outJson))
                {
                    bool serverExists = false;
                    if (outJson.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        serverExists = false;
                    }
                    else
                    {
                        var outJObject = JObject.Parse(outJson);

                        if (outJObject.ContainsKey("passwordRequired"))
                        {
                            errorMessage = "passwordRequired";
                            return false;
                        }

                        if (outJObject.ContainsKey("invalidPassword"))
                        {
                            errorMessage = "Invalid password";
                            return false;
                        }

                        if (outJObject.ContainsKey("gameVersion"))
                        {
                            if (JObject.Parse(outJson)["gameVersion"].ToString() != StayInTarkovPlugin.EFTVersionMajor)
                            {
                                errorMessage = $"You are attempting to use a different version of EFT {StayInTarkovPlugin.EFTVersionMajor} than what the server is running {JObject.Parse(outJson)["gameVersion"]}";
                                return false;
                            }
                        }

                        if (outJObject.ContainsKey("sitVersion"))
                        {
                            if (JObject.Parse(outJson)["sitVersion"].ToString() != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                            {
                                errorMessage = $"You are attempting to use a different version of SIT {Assembly.GetExecutingAssembly().GetName().Version.ToString()} than what the server is running {JObject.Parse(outJson)["sitVersion"]}";
                                return false;
                            }
                        }

                        serverExists = true;
                    }
                    StayInTarkovHelperConstants.Logger.LogInfo($"CheckForMatch:Server Exists?:{serverExists}");

                    return serverExists;
                }
            }
            return false;
        }

        public static bool JoinMatch(RaidSettings settings, string profileId, string serverId, string password, out string outJson, out string errorMessage)
        {
            errorMessage = $"No server matches the data provided or the server no longer exists";
            StayInTarkovHelperConstants.Logger.LogDebug("JoinMatch");
            outJson = string.Empty;

            if (MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance != null)
            {
                JObject objectToSend = JObject.FromObject(settings);
                objectToSend.Add("profileId", profileId);
                objectToSend.Add("serverId", serverId);
                objectToSend.Add("password", password);

                outJson = AkiBackendCommunication.Instance.PostJson("/coop/server/join", objectToSend.ToJson());
                StayInTarkovHelperConstants.Logger.LogInfo(outJson);

                if (!string.IsNullOrEmpty(outJson))
                {
                    if (outJson.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = $"An unknown SPT-Aki Server error has occurred";
                        return false;
                    }

                    var outJObject = JObject.Parse(outJson);
                    if (outJObject.ContainsKey("passwordRequired"))
                    {
                        errorMessage = "passwordRequired";
                        return false;
                    }

                    if (outJObject.ContainsKey("invalidPassword"))
                    {
                        errorMessage = "Invalid password";
                        return false;
                    }

                    if (outJObject.ContainsKey("alreadyConnected"))
                    {
                        errorMessage = "Your profile is already connected to this server";
                        return false;
                    }

                    if (outJObject.ContainsKey("gameVersion"))
                    {
                        if (JObject.Parse(outJson)["gameVersion"].ToString() != StayInTarkovPlugin.EFTVersionMajor)
                        {
                            errorMessage = $"You are attempting to use a different version of EFT {StayInTarkovPlugin.EFTVersionMajor} than what the server is running {JObject.Parse(outJson)["gameVersion"]}";
                            return false;
                        }
                    }

                    if (outJObject.ContainsKey("sitVersion"))
                    {
                        if (JObject.Parse(outJson)["sitVersion"].ToString() != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            errorMessage = $"You are attempting to use a different version of SIT {Assembly.GetExecutingAssembly().GetName().Version.ToString()} than what the server is running {JObject.Parse(outJson)["sitVersion"]}";
                            return false;
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        //public static void CreateMatch(string accountId, RaidSettings rs)
        public static void CreateMatch(string profileId, RaidSettings rs, string password = null)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            var objectToSend = new Dictionary<string, object>
            {
                { "serverId", profileId }
                , { "serverType", PluginConfigSettings.Instance.CoopSettings.SITServerType }
                , { "serverUdpPort", PluginConfigSettings.Instance.CoopSettings.SITUdpPort }
                , { "timestamp", timestamp }
                , { "settings", rs }
                , { "expectedNumberOfPlayers", MatchmakerAcceptPatches.HostExpectedNumberOfPlayers }
                , { "gameVersion", StayInTarkovPlugin.EFTVersionMajor }
                , { "sitVersion", Assembly.GetExecutingAssembly().GetName().Version }
            };

            if (password != null)
                objectToSend.Add("password", password);

            string result = AkiBackendCommunication.Instance.PostJson("/coop/server/create", JsonConvert.SerializeObject(
                objectToSend));

            if (!string.IsNullOrEmpty(result))
            {
                JObject outJObject = JObject.Parse(result);

                StayInTarkovHelperConstants.Logger.LogInfo($"CreateMatch:: Match Created for {profileId}");
                SetGroupId(profileId);
                ServerType = PluginConfigSettings.Instance.CoopSettings.SITServerType;
                ServerPort = PluginConfigSettings.Instance.CoopSettings.SITUdpPort;
                SetTimestamp(timestamp);
                MatchingType = EMatchmakerType.GroupLeader;
                return;
            }

            StayInTarkovHelperConstants.Logger.LogError("CreateMatch:: ERROR: Match NOT Created");

        }
    }
}
