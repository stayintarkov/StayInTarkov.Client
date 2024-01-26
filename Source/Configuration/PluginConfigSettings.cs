using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Net;

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

            public long SITGCMemoryThreshold { get; internal set; }

            public bool SITGCClearAssets { get; internal set; }

            public bool SITGCAggressiveClean { get; internal set; }

            public void GetSettings()
            {
                UseSITGarbageCollector = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "UseSITGarbageCollector", true, new ConfigDescription("Whether to use the Garbage Collector developed in to SIT OR leave it to BSG/Unity")).Value;

                SITGCMemoryThreshold = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "SITGarbageCollectorMemoryThreshold", 512, new ConfigDescription("The SIT Garbage Collector memory threshold (in megabytes) between ticks before forcing a garbage collection")).Value;

                SITGCClearAssets = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "SITGarbageCollectorClearAssets", false, new ConfigDescription("Set SIT Garbage Collector to clear Unity assets. Reduces RAM usage but can be unstable!")).Value;

                SITGCAggressiveClean = StayInTarkovPlugin.Instance.Config.Bind
                ("Advanced", "SITGarbageCollectorAggressiveClean", false, new ConfigDescription("Set SIT Garbage Collector to aggresively clean RAM. This will signficantly reduce in Raid RAM usage at the expense of a 1-5s freeze.")).Value;

                Logger.LogInfo($"UseSITGarbageCollector:{UseSITGarbageCollector}");
                Logger.LogInfo($"SITGCMemoryThreshold:{SITGCMemoryThreshold}");
                Logger.LogInfo($"SITGCClearAssets:{SITGCClearAssets}");
                Logger.LogInfo($"SITGCAggressiveClean:{SITGCAggressiveClean}");

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

            public int SETTING_PlayerStateTickRateInMS { get; set; } = 150;
            public bool SETTING_HeadshotsAlwaysKill { get; set; } = false;
            public bool SETTING_ShowFeed { get; set; } = true;
            public bool SETTING_ShowSITStatistics { get; set; } = true;
            public HostProtocol SITHostProtocol { get; private set; }
            public int SITWebSocketPort { get; set; } = 6970;
            public int SITUDPPort { get; set; } = 6971;

            public bool AllPlayersSpawnTogether { get; set; } = true;
            public bool ArenaMode { get; set; } = false;
            public bool EnableAISpawnWaveSystem { get; set; } = true;

            public bool ForceHighPingMode { get; set; } = false;
            public bool RunThroughOnServerStop { get; set; } = true;

            public int BlackScreenOnDeathTime
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       ("Coop", "BlackScreenOnDeathTime", 333, new ConfigDescription("How long to wait until your death waits to become a Free Camera")).Value;
                }
            }
            
            public bool QuickStartSolo { get; set; } = true;

            public string SITUDPHostIPV4 { get; set; }
            public string SITUDPHostIPV6 { get; set; }

            public void GetSettings()
            {
                SETTING_DEBUGSpawnDronesOnServer = StayInTarkovPlugin.Instance.Config.Bind
                ("Coop", "ShowDronesOnServer", false, new ConfigDescription("Whether to spawn the client drones on the server -- for debugging")).Value;

                SETTING_PlayerStateTickRateInMS = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "PlayerStateTickRateInMS", 333, new ConfigDescription("TCP Only: The rate at which Player States will attempt to be synchronized. Min: 150ms, Max 666ms")).Value;
                SETTING_PlayerStateTickRateInMS = Math.Min(666, Math.Max(150, SETTING_PlayerStateTickRateInMS));

                SETTING_HeadshotsAlwaysKill = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "HeadshotsAlwaysKill", false, new ConfigDescription("Enable to make headshots actually work, no more tanking definite kills!")).Value;

                SETTING_ShowFeed = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "ShowFeed", true, new ConfigDescription("Enable the feed on the bottom right of the screen which shows player/bot spawns, kills, etc.")).Value;


                AllPlayersSpawnTogether = StayInTarkovPlugin.Instance.Config.Bind
               ("Coop", "AllPlayersSpawnTogether", true, new ConfigDescription("Whether to spawn all players in the same place")).Value;

                ArenaMode = StayInTarkovPlugin.Instance.Config.Bind
                ("Coop", "ArenaMode", false, new ConfigDescription("Arena Mode - For the meme's (DEBUG). Can SIT be less laggy than Live Tarkov in PvP?")).Value;

                EnableAISpawnWaveSystem = StayInTarkovPlugin.Instance.Config.Bind("Coop", "EnableAISpawnWaveSystem", true
                        , new ConfigDescription("Whether to run the Wave Spawner System. If this is False. No AI will spawn. Useful for testing in a PvP only environment.")).Value;

                ForceHighPingMode = StayInTarkovPlugin.Instance.Config.Bind("Coop", "ForceHighPingMode", false
                        , new ConfigDescription("Forces the High Ping Mode which allows some actions to not round-trip. This may be useful if you have large input lag")).Value;

                SETTING_ShowSITStatistics = StayInTarkovPlugin.Instance.Config.Bind
                 ("Coop", "ShowSITStatistics", true, new ConfigDescription("Enable the SIT statistics on the top left of the screen which shows ping, player count, etc.")).Value;

                QuickStartSolo = StayInTarkovPlugin.Instance.Config.Bind
                    ("Coop", "QuickStart", true, new ConfigDescription("Replaces single player with a quick start server hosted for 1 player.")).Value;

                Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
                Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
                Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
                Logger.LogDebug($"SETTING_HeadshotsAlwaysKill: {SETTING_HeadshotsAlwaysKill}");
                Logger.LogDebug($"SETTING_ShowFeed: {SETTING_ShowFeed}");
                Logger.LogDebug($"SETTING_ShowFeed: {SETTING_ShowSITStatistics}");
                Logger.LogDebug($"AllPlayersSpawnTogether: {AllPlayersSpawnTogether}");
                Logger.LogDebug($"ArenaMode: {ArenaMode}");
                Logger.LogDebug($"ForceHighPingMode: {ForceHighPingMode}");

                SITHostProtocol = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITHostProtocol", HostProtocol.TCP, new ConfigDescription("SIT Host Protocol.")).Value;
                SITWebSocketPort = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITPort", 6970, new ConfigDescription("SIT TCP/Websocket Port DEFAULT = 6970")).Value;
                SITUDPPort = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITUDPPort", 6971, new ConfigDescription("SIT UDP Port DEFAULT = 6971")).Value;
                SITUDPHostIPV4 = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITUDPHostIPV4", "127.0.0.1", new ConfigDescription("The IPv4 to use when hosting a UDP Coop Session")).Value;
                SITUDPHostIPV6 = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITUDPHostIPV6", "2001:0db8:85a3:0000:0000:8a2e:0370:7334", new ConfigDescription("The IPv6 to use when hosting a UDP Coop Session")).Value;

                Logger.LogDebug($"SITWebSocketPort: {SITWebSocketPort}");

                if (ArenaMode)
                {
                    Logger.LogInfo($"x!Arena Mode Activated!x");
                    AllPlayersSpawnTogether = false;
                    EnableAISpawnWaveSystem = false;
                }
            }

            public enum HostProtocol
            {
                TCP,
                UDP, 
                //Both
            }
        }

    }
}
