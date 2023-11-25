﻿using BepInEx.Configuration;
using BepInEx.Logging;

namespace StayInTarkov.Configuration
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Stores and Loads all of the Plugin config settings
    /// </summary>
    public class PluginConfigSettings
    {
        public ConfigFile Config { get; }
        public ManualLogSource Logger { get; }

        public static PluginConfigSettings Instance { get; private set; }

        public CoopConfigSettings CoopSettings { get; }

        public SITAdvancedSettings AdvancedSettings { get; }

        public PluginConfigSettings(ManualLogSource logger, ConfigFile config)
        {
            Logger = logger;
            Config = config;
            CoopSettings = new CoopConfigSettings(logger, config);
            AdvancedSettings = new SITAdvancedSettings(logger, config);
            Instance = this;
        }

        public void GetSettings()
        {

        }

        public class SITAdvancedSettings
        {
            public ConfigFile Config { get; }
            public ManualLogSource Logger { get; }

            public SITAdvancedSettings(ManualLogSource logger, ConfigFile config)
            {
                Logger = logger;
                Config = config;
                GetSettings();
            }

            public bool UseSITGarbageCollector { get; set; }

            public int SITGarbageCollectorIntervalMinutes { get; set; }

            public void GetSettings()
            {
                UseSITGarbageCollector = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "UseSITGarbageCollector", true, new ConfigDescription("Whether to use the Garbage Collector developed in to SIT OR leave it to BSG/Unity")).Value;

                SITGarbageCollectorIntervalMinutes = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "SITGarbageCollectorIntervalMinutes", 5, new ConfigDescription("Number of minutes to enable SIT Garbage Collector")).Value;

                Logger.LogInfo($"UseSITGarbageCollector:{UseSITGarbageCollector}");
                Logger.LogInfo($"SITGarbageCollectorIntervalMinutes:{SITGarbageCollectorIntervalMinutes}");

            }
        }

        public class CoopConfigSettings
        {
            public ConfigFile Config { get; }
            public ManualLogSource Logger { get; }

            public CoopConfigSettings(ManualLogSource logger, ConfigFile config)
            {
                Logger = logger;
                Config = config;
                GetSettings();
            }

            public bool SETTING_DEBUGSpawnDronesOnServer { get; set; } = false;
            public bool SETTING_DEBUGShowPlayerList
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       ("Coop", "ShowPlayerList", false, new ConfigDescription("Whether to show the player list on the GUI -- for debugging")).Value;
                }
            }

            public int SETTING_PlayerStateTickRateInMS { get; set; } = 333;
            public bool SETTING_HeadshotsAlwaysKill { get; set; } = false;
            public bool SETTING_ShowFeed { get; set; } = true;
            public bool SETTING_ShowSITStatistics { get; set; } = true;
            public int SITWebSocketPort { get; set; } = 6970;

            public bool AllPlayersSpawnTogether { get; set; } = true;
            public bool ArenaMode { get; set; } = false;
            public bool EnableAISpawnWaveSystem { get; set; } = true;

            public bool ForceHighPingMode { get; set; } = false;
            public bool RunThroughOnServerStop { get; set; } = true;

            public bool BotWavesDisableStopper { get; set; } = false;

            public int BlackScreenOnDeathTime
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       ("Coop", "BlackScreenOnDeathTime", 333, new ConfigDescription("How long to wait until your death waits to become a Free Camera")).Value;
                }
            }

            public void GetSettings()
            {
                SETTING_DEBUGSpawnDronesOnServer = StayInTarkovPlugin.Instance.Config.Bind
                ("Coop", "ShowDronesOnServer", false, new ConfigDescription("Whether to spawn the client drones on the server -- for debugging")).Value;

                //SETTING_DEBUGShowPlayerList = 

                SETTING_PlayerStateTickRateInMS = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "PlayerStateTickRateInMS", 666, new ConfigDescription("The rate at which Player States will be synchronized")).Value;
                SETTING_PlayerStateTickRateInMS = 666;

                SETTING_HeadshotsAlwaysKill = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "HeadshotsAlwaysKill", false, new ConfigDescription("Enable to make headshots actually work, no more tanking definite kills!")).Value;

                SETTING_ShowFeed = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "ShowFeed", true, new ConfigDescription("Enable the feed on the bottom right of the screen which shows player/bot spawns, kills, etc.")).Value;

                SITWebSocketPort = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITPort", 6970, new ConfigDescription("SIT.Core Websocket Port DEFAULT = 6970")).Value;

                AllPlayersSpawnTogether = StayInTarkovPlugin.Instance.Config.Bind
               ("Coop", "AllPlayersSpawnTogether", true, new ConfigDescription("Whether to spawn all players in the same place")).Value;

                ArenaMode = StayInTarkovPlugin.Instance.Config.Bind
                ("Coop", "ArenaMode", false, new ConfigDescription("Arena Mode - For the meme's (DEBUG). Can SIT be less laggy than Live Tarkov in PvP?")).Value;

                EnableAISpawnWaveSystem = StayInTarkovPlugin.Instance.Config.Bind("Coop", "EnableAISpawnWaveSystem", true
                        , new ConfigDescription("Whether to run the Wave Spawner System. If this is False. No AI will spawn. Useful for testing in a PvP only environment.")).Value;

                ForceHighPingMode = StayInTarkovPlugin.Instance.Config.Bind("Coop", "ForceHighPingMode", false
                        , new ConfigDescription("Forces the High Ping Mode which allows some actions to not round-trip. This may be useful if you have large input lag")).Value;

                RunThroughOnServerStop = StayInTarkovPlugin.Instance.Config.Bind("Coop", "RunThroughOnServerStop", true
                        , new ConfigDescription("Controls whether clients still in-raid when server dies will receive a Run Through (true) or Survived (false).")).Value;

                BotWavesDisableStopper = StayInTarkovPlugin.Instance.Config.Bind("Coop", "BotWavesDisableStopper", false
                        , new ConfigDescription("Disable the function StopBotSpawningAfterTimer, so not gonna disable bot spawning after 180 sec")).Value;

                SETTING_ShowSITStatistics = StayInTarkovPlugin.Instance.Config.Bind
                 ("Coop", "ShowSITStatistics", true, new ConfigDescription("Enable the SIT statistics on the top left of the screen which shows ping, player count, etc.")).Value;

                Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
                Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
                Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
                Logger.LogDebug($"SETTING_HeadshotsAlwaysKill: {SETTING_HeadshotsAlwaysKill}");
                Logger.LogDebug($"SETTING_ShowFeed: {SETTING_ShowFeed}");
                Logger.LogDebug($"SETTING_ShowFeed: {SETTING_ShowSITStatistics}");
                Logger.LogDebug($"SITWebSocketPort: {SITWebSocketPort}");
                Logger.LogDebug($"AllPlayersSpawnTogether: {AllPlayersSpawnTogether}");
                Logger.LogDebug($"ArenaMode: {ArenaMode}");
                Logger.LogDebug($"ForceHighPingMode: {ForceHighPingMode}");
                Logger.LogDebug($"RunThroughOnServerStop: {RunThroughOnServerStop}");

                if (ArenaMode)
                {
                    Logger.LogInfo($"x!Arena Mode Activated!x");
                    AllPlayersSpawnTogether = false;
                    EnableAISpawnWaveSystem = false;
                }
            }
        }

    }
}
