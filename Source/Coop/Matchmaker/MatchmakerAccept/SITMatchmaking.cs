﻿using BepInEx.Logging;
using EFT;
using EFT.UI.Matchmaker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Configuration;
using StayInTarkov.Networking;
using STUN;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace StayInTarkov.Coop.Matchmaker
{
    public enum EMatchmakerType
    {
        Single = 0,
        GroupPlayer = 1,
        GroupLeader = 2
    }

    public static class SITMatchmaking
    {
        #region Fields/Properties
        public static EFT.UI.Matchmaker.MatchMakerAcceptScreen? MatchMakerAcceptScreenInstance { get; set; }
        public static Profile? Profile { get; set; }
        public static EMatchmakerType MatchingType { get; set; } = EMatchmakerType.Single;
        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;
        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;
        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static int HostExpectedNumberOfPlayers { get; set; } = 1;
        private static string? groupId;
        private static long timestamp;
        #endregion

        #region Static Fields

        public static object? MatchmakerScreenController
        {
            get
            {
                var screenController = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(MatchMakerAcceptScreenInstance, "ScreenController", false);
                if (screenController != null)
                {
                    Logger.LogInfo("MatchmakerAcceptPatches.Found ScreenController Instance");

                    return screenController;

                }
                return null;
            }
        }

        public static TimeHasComeScreenController? TimeHasComeScreenController { get; internal set; }
        public static ESITProtocol SITProtocol { get; internal set; } = ESITProtocol.RelayTcp;
        public static string PublicIPAddress { get; internal set; } = "";
        public static int PublicPort { get; internal set; } = 6972;
        public static ManualLogSource Logger { get; }
        #endregion

        static SITMatchmaking()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITMatchmaking));
        }

        public static void Run()
        {
            new MatchmakerAcceptScreenAwakePatch().Enable();
            new MatchmakerAcceptScreenShowPatch().Enable();
        }

        public static string? GetGroupId()
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
            Logger.LogDebug(nameof(CheckForMatch));

            errorMessage = ((string?)StayInTarkovPlugin.LanguageDictionary["NO-SERVER-MATCH"]) ?? "NO-SERVER-MATCH";
            outJson = string.Empty;

            if (SITMatchmaking.MatchMakerAcceptScreenInstance != null)
            {
                JObject settingsJSON = JObject.FromObject(settings);
                settingsJSON.Add("password", password);

                outJson = AkiBackendCommunication.Instance.PostJson("/coop/server/exist", JsonConvert.SerializeObject(settingsJSON));
                Logger.LogInfo(outJson);

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
                            errorMessage = (string?)StayInTarkovPlugin.LanguageDictionary["INVALID-PASSWORD"] ?? "INVALID-PASSWORD";
                            return false;
                        }

                        if (outJObject.ContainsKey("gameVersion"))
                        {
                            if ((string?)JObject.Parse(outJson)["gameVersion"] != StayInTarkovPlugin.EFTVersionMajor)
                            {
                                errorMessage = $"{StayInTarkovPlugin.LanguageDictionary["USE-A-DIFFERENT-VERSION-OF-EFT"]} {StayInTarkovPlugin.EFTVersionMajor} {StayInTarkovPlugin.LanguageDictionary["THAN-SERVER-RUNNING"]} {JObject.Parse(outJson)["gameVersion"]}";
                                return false;
                            }
                        }

                        if (outJObject.ContainsKey("sitVersion"))
                        {
                            if ((string?)JObject.Parse(outJson)["sitVersion"] != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                            {
                                errorMessage = $"{StayInTarkovPlugin.LanguageDictionary["USE-A-DIFFERENT-VERSION-OF-SIT"]} {Assembly.GetExecutingAssembly().GetName().Version.ToString()} {StayInTarkovPlugin.LanguageDictionary["THAN-SERVER-RUNNING"]} {JObject.Parse(outJson)["sitVersion"]}";
                                return false;
                            }
                        }

                        serverExists = true;
                    }
                    Logger.LogInfo($"CheckForMatch:Server Exists?:{serverExists}");

                    return serverExists;
                }
            }
            return false;
        }

        public static bool TryJoinMatch(RaidSettings settings, string profileId, string serverId, string password, out string outJson, out string errorMessage)
        {
            errorMessage = (string?)StayInTarkovPlugin.LanguageDictionary["NO-SERVER-MATCH"] ?? "NO-SERVER-MATCH";
            Logger.LogDebug("JoinMatch");
            outJson = string.Empty;

            if (SITMatchmaking.MatchMakerAcceptScreenInstance != null)
            {
                JObject objectToSend = JObject.FromObject(settings);
                objectToSend.Add("profileId", profileId);
                objectToSend.Add("serverId", serverId);
                objectToSend.Add("password", password);

                outJson = AkiBackendCommunication.Instance.PostJson("/coop/server/join", objectToSend.ToJson());
                Logger.LogInfo(outJson);

                if (!string.IsNullOrEmpty(outJson))
                {
                    if (outJson.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = (string?)StayInTarkovPlugin.LanguageDictionary["SPT-AKI-SERVER-ERROR"] ?? "SPT-AKI-SERVER-ERROR";
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
                        errorMessage = (string?)StayInTarkovPlugin.LanguageDictionary["INVALID-PASSWORD"] ?? "INVALID-PASSWORD";
                        return false;
                    }

                    if (outJObject.ContainsKey("alreadyConnected"))
                    {
                        errorMessage = (string?)StayInTarkovPlugin.LanguageDictionary["PROFILE-IS-ALREADY"] ?? "PROFILE-IS-ALREADY";
                        return false;
                    }

                    if (outJObject.ContainsKey("gameVersion"))
                    {
                        if ((string?)JObject.Parse(outJson)["gameVersion"] != StayInTarkovPlugin.EFTVersionMajor)
                        {
                            errorMessage = $"{StayInTarkovPlugin.LanguageDictionary["USE-A-DIFFERENT-VERSION-OF-EFT"]} {StayInTarkovPlugin.EFTVersionMajor} {StayInTarkovPlugin.LanguageDictionary["THAN-SERVER-RUNNING"]} {JObject.Parse(outJson)["gameVersion"]}";
                            return false;
                        }
                    }

                    if (outJObject.ContainsKey("sitVersion"))
                    {
                        if ((string?)JObject.Parse(outJson)["sitVersion"] != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            errorMessage = $"{StayInTarkovPlugin.LanguageDictionary["USE-A-DIFFERENT-VERSION-OF-SIT"]} {Assembly.GetExecutingAssembly().GetName().Version.ToString()} {StayInTarkovPlugin.LanguageDictionary["THAN-SERVER-RUNNING"]} {JObject.Parse(outJson)["sitVersion"]}";
                            return false;
                        }
                    }
                    
                    MatchingType = EMatchmakerType.GroupPlayer;

                    return true;
                }
            }
            return false;
        }

        public static void CreateMatch(string profileId
            , RaidSettings rs
            , string password
            , ESITProtocol protocol
            , string ipAddress
            , int port
            , EMatchmakerType matchmakerType)
        {           
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            SITProtocol = protocol;

            var objectToSend = new Dictionary<string, object>
            {
                { "serverId", profileId },
                { "timestamp", timestamp },
                { "settings", rs },
                { "expectedNumberOfPlayers", HostExpectedNumberOfPlayers },
                { "gameVersion", StayInTarkovPlugin.EFTVersionMajor },
                { "sitVersion", Assembly.GetExecutingAssembly().GetName().Version },
                { "protocol", protocol },
                { "port", port } // NOTE(belette) only needed so that nodejs nathelper can set the "remote" endpoint
            };

            if (!string.IsNullOrEmpty(password))
                objectToSend.Add("password", password);

            Logger.LogDebug($"{nameof(CreateMatch)}");
            Logger.LogDebug($"{objectToSend.ToJson()}");

            // KWJimWails: Set request timeout to 20 seconds, because the Streets of Tarkov need a long time to create loot infos.
            string result = AkiBackendCommunication.Instance.PostJson("/coop/server/create", JsonConvert.SerializeObject(objectToSend), true, 20000);

            if (string.IsNullOrEmpty(result))
            {
                Logger.LogError("CreateMatch:: ERROR: Match NOT Created");
                return;
            }

            Logger.LogDebug($"{nameof(CreateMatch)}: Match Created for {profileId}");

            SetGroupId(profileId);
            SetTimestamp(timestamp);
            MatchingType = matchmakerType;
            PublicIPAddress = ipAddress;
            PublicPort = port;
        }
    }
}
