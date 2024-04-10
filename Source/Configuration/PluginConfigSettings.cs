using BepInEx.Configuration;
using BepInEx.Logging;
using StayInTarkov.Networking;
using System;
using System.Net;

#nullable enable

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

        public static PluginConfigSettings? Instance { get; private set; }

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
            public const string Advanced = "Advanced";
            public ConfigFile Config { get; }
            public ManualLogSource Logger { get; }

            public bool SETTING_EnableSITGC
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       (Advanced, "EnableSITGC", false, new ConfigDescription("Enable SIT's own Garbage Collector")).Value;
                }
            }

            public uint SETTING_SITGCMemoryThreshold
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       (Advanced, "SITGCMemoryThreshold", 90u, new ConfigDescription("SIT's Garbage Collector. System Memory % before SIT forces a Garbage Collection.")).Value;
                }
            }


            public SITAdvancedSettings(ManualLogSource logger, ConfigFile config)
            {
                Logger = logger;
                Config = config;
                _ = SETTING_EnableSITGC;
                _ = SETTING_SITGCMemoryThreshold;
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
            public bool ShowPing { get; set; } = true;
            public int SITWebSocketPort { get; set; } = 6970;
            public int SITNatHelperPort { get; set; } = 6971;
            public string UdpServerLocalIPv4 { get; set; } = "0.0.0.0";
            public string UdpServerLocalIPv6 { get; set; } = "::";
            public int UdpServerLocalPort { get; set; } = 6972;
            public string UdpServerPublicIP { get; set; } = "";
            public int UdpServerPublicPort { get; set; } = 0;
            //public ServerType SITServerType { get; set; } = ServerType.Relay;
            public NatTraversalMethod SITNatTraversalMethod { get; set; } = NatTraversalMethod.Upnp;

            public bool AllPlayersSpawnTogether { get; set; } = true;
            public bool ArenaMode { get; set; } = false;
            public bool EnableAISpawnWaveSystem { get; set; } = true;

            public bool ForceHighPingMode { get; set; } = false;
            public bool RunThroughOnServerStop { get; set; } = true;

            public int WaitingTimeBeforeStart { get; private set; }

            public int BlackScreenOnDeathTime
            {
                get
                {
                    return StayInTarkovPlugin.Instance.Config.Bind
                       ("Coop", "BlackScreenOnDeathTime", 500, new ConfigDescription("How long to wait until your death waits to become a Free Camera")).Value;
                }
            }

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

                WaitingTimeBeforeStart = Config.Bind("Coop", "WaitingTimeBeforeStart", 120
                        , new ConfigDescription("Time in seconds to wait for players before starting the game automatically")).Value;

                SETTING_ShowSITStatistics = StayInTarkovPlugin.Instance.Config.Bind
                 ("Coop", "ShowSITStatistics", true, new ConfigDescription("Enable the SIT statistics on the top left of the screen which shows ping, player count, etc.")).Value;

                ShowPing = StayInTarkovPlugin.Instance.Config.Bind
                    ("Coop", "ShowPing", true, new ConfigDescription("Enables RTT display in the top left of the screen.")).Value;

                SITWebSocketPort = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITPort", 6970, new ConfigDescription("SIT TCP/Websocket Port")).Value;
                SITNatHelperPort = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITNatHelperPort", 6971, new ConfigDescription("SIT Nat Helper Port")).Value;
                //SITServerType = StayInTarkovPlugin.Instance.Config.Bind("Coop", "SITServerType", ServerType.Relay, new ConfigDescription("SIT Server Type (when hosting a match). Possible values: Relay, P2P")).Value;

                UdpServerLocalIPv4 = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "UdpServerLocalIPv4", "0.0.0.0", new ConfigDescription("Peer-to-peer (UDP) only: Default IPv4 address to bind to when listening for connections")).Value;

                UdpServerLocalIPv6 = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "UdpServerLocalIPv6", "::", new ConfigDescription("Peer-to-peer (UDP) only: Default IPv6 address to bind to when listening for connections")).Value;

                UdpServerLocalPort = StayInTarkovPlugin.Instance.Config.Bind
                  ("Coop", "UdpServerLocalPort", 6972, new ConfigDescription("Peer-to-peer (UDP) only: Default Port to bind to when listening for connections")).Value;

                UdpServerPublicIP = StayInTarkovPlugin.Instance.Config.Bind<string>
                  ("Coop", "UdpServerPublicIP", "", new ConfigDescription("Peer-to-peer (UDP) only: Default IP address to advertise to peers when listening for connections")).Value;

                UdpServerPublicPort = StayInTarkovPlugin.Instance.Config.Bind<int>
                  ("Coop", "UdpServerPublicPort", 0, new ConfigDescription("Peer-to-peer (UDP) only: Default Port to advertise to peers when listening for connections")).Value;

                Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
                Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
                Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
                Logger.LogDebug($"SETTING_HeadshotsAlwaysKill: {SETTING_HeadshotsAlwaysKill}");
                Logger.LogDebug($"SETTING_ShowFeed: {SETTING_ShowFeed}");
                Logger.LogDebug($"SETTING_ShowSITStatistics: {SETTING_ShowSITStatistics}");
                Logger.LogDebug($"ShowPing: {ShowPing}");
                Logger.LogDebug($"AllPlayersSpawnTogether: {AllPlayersSpawnTogether}");
                Logger.LogDebug($"ArenaMode: {ArenaMode}");
                Logger.LogDebug($"ForceHighPingMode: {ForceHighPingMode}");
                Logger.LogDebug($"SITWebSocketPort: {SITWebSocketPort}");
                Logger.LogDebug($"SITNatHelperPort: {SITNatHelperPort}");
                Logger.LogDebug($"UdpServerLocalIPv4: {UdpServerLocalIPv4}");
                Logger.LogDebug($"UdpServerLocalIPv6: {UdpServerLocalIPv6}");
                Logger.LogDebug($"UdpServerLocalPort: {UdpServerLocalPort}");
                Logger.LogDebug($"UdpServerPublicIP: {UdpServerPublicIP}");
                Logger.LogDebug($"UdpServerPublicPort: {UdpServerPublicPort}");

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
