using Aki.Custom.Airdrops;
using Aki.Custom.Airdrops.Models;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using EFT.UI.BattleTimer;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Coop.World;
//using StayInTarkov.Core.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace StayInTarkov.Coop.Components
{
    public class ActionPacketHandlerComponent : MonoBehaviour
    {
        public readonly BlockingCollection<ISITPacket> ActionSITPackets = new(9999);

        public readonly BlockingCollection<Dictionary<string, object>> ActionPackets = new(9999);
        public BlockingCollection<Dictionary<string, object>> ActionPacketsMovement { get; private set; } = new(9999);
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;
        public ManualLogSource Logger { get; private set; }

        private SITGameComponent CoopGameComponent { get; set; }

        void Awake()
        {
            // ----------------------------------------------------
            // Create a BepInEx Logger for ActionPacketHandlerComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource("ActionPacketHandlerComponent");
            Logger.LogDebug("Awake");
            ActionPacketsMovement = new();
        }

        void Start()
        {
            CoopGameComponent = this.gameObject.GetComponent<SITGameComponent>();
            ActionPacketsMovement = new();
        }

        void Update()
        {
            ProcessActionPackets();
        }

        private void ProcessActionPackets()
        {
            CoopGameComponent = gameObject.GetComponent<SITGameComponent>();

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (ActionPackets == null)
                return;

            if (Players == null)
                return;

            if (ActionSITPackets.Count > 0)
            {
#if DEBUGPACKETS
                Stopwatch stopwatchActionPackets = Stopwatch.StartNew();
#endif
                while (ActionSITPackets.TryTake(out var packet))
                {
#if DEBUGPACKETS
                    Stopwatch stopwatchActionPacket = Stopwatch.StartNew();
#endif
                    packet.Process();

#if DEBUGPACKETS
                    if (stopwatchActionPacket.ElapsedMilliseconds > 1)
                        Logger.LogDebug($"ActionSITPacket {packet.Method} took {stopwatchActionPacket.ElapsedMilliseconds}ms to process!");
#endif
                }
#if DEBUGPACKETS
                if (stopwatchActionPackets.ElapsedMilliseconds > 1)
                    Logger.LogDebug($"ActionSITPackets took {stopwatchActionPackets.ElapsedMilliseconds}ms to process!");
#endif
            }

            if (ActionPackets.Count > 0)
            {
#if DEBUGPACKETS
                Stopwatch stopwatchActionPackets = Stopwatch.StartNew();
#endif
                while (ActionPackets.TryTake(out var result))
                {
#if DEBUGPACKETS
                    Stopwatch stopwatchActionPacket = Stopwatch.StartNew();
#endif
                    if (!ProcessLastActionDataPacket(result))
                    {
                        continue;
                    }

#if DEBUGPACKETS
                    if (stopwatchActionPacket.ElapsedMilliseconds > 1)
                        Logger.LogDebug($"ActionPacket {result["m"]} took {stopwatchActionPacket.ElapsedMilliseconds}ms to process!");
#endif
                }
#if DEBUGPACKETS
                if (stopwatchActionPackets.ElapsedMilliseconds > 1)
                    Logger.LogDebug($"ActionPackets took {stopwatchActionPackets.ElapsedMilliseconds}ms to process!");
#endif
            }

            if (ActionPacketsMovement != null && ActionPacketsMovement.Count > 0)
            {
#if DEBUGPACKETS
                Stopwatch stopwatchActionPacketsMovement = Stopwatch.StartNew();
#endif
                while (ActionPacketsMovement.TryTake(out var result))
                {
                    if (!ProcessLastActionDataPacket(result))
                    {
                        //ActionPacketsMovement.Add(result);
                        continue;
                    }
                }
#if DEBUGPACKETS
                if (stopwatchActionPacketsMovement.ElapsedMilliseconds > 1)
                {
                    Logger.LogDebug($"ActionPacketsMovement took {stopwatchActionPacketsMovement.ElapsedMilliseconds}ms to process!");
                }
#endif
            }


            return;
        }

        bool ProcessLastActionDataPacket(Dictionary<string, object> packet)
        {
            if (Singleton<GameWorld>.Instance == null)
                return false;

            if (packet == null || packet.Count == 0)
            {
                Logger.LogInfo("No Data Returned from Last Actions!");
                return false;
            }
            

            bool result = ProcessPlayerPacket(packet);
            if (!result)
                result = ProcessWorldPacket(ref packet);

            return result;
        }

        bool ProcessWorldPacket(ref Dictionary<string, object> packet)
        {
            // this isn't a world packet. return true
            if (packet.ContainsKey("profileId"))
                return true;

            // this isn't a world packet. return true
            if (!packet.ContainsKey("m"))
                return true;

            var result = false;
            string method = packet["m"].ToString();

            foreach (var coopPatch in CoopPatches.NoMRPPatches)
            {
                if (coopPatch is IModuleReplicationWorldPatch imrwp)
                {
                    if (imrwp.MethodName == method)
                    {
                        imrwp.Replicated(ref packet);
                        result = true;
                    }
                }
            }

            switch (method)
            {
                case "AirdropPacket":
                    ReplicateAirdrop(packet);
                    result = true;
                    break;
                case "AirdropLootPacket":
                    ReplicateAirdropLoot(packet);
                    result = true;
                    break;
                //case "RaidTimer":
                //    ReplicateRaidTimer(packet);
                //    result = true;
                //    break;
                //case "TimeAndWeather":
                //    ReplicateTimeAndWeather(packet);
                //    result = true;
                //    break;
                case "ArmoredTrainTime":
                    ReplicateArmoredTrainTime(packet);
                    result = true;
                    break;
                case "LootableContainer_Interact":
                    LootableContainer_Interact_Patch.Replicated(packet);
                    result = true;
                    break;
            }

            return result;
        }

        bool ProcessPlayerPacket(Dictionary<string, object> packet)
        {

            if (packet == null)
                return true;

            var profileId = "";
            
            if (packet.ContainsKey("profileId"))
                profileId = packet["profileId"].ToString();

            if (packet.ContainsKey("ProfileId"))
                profileId = packet["ProfileId"].ToString();

            if (string.IsNullOrEmpty(profileId))
                return false;

            if (Players == null)
            {
                Logger.LogDebug("Players is Null");
                return false;
            }

            if (Players.Count == 0)
            {
                Logger.LogDebug("Players is Empty");
                return false;
            }

            if (!Players.ContainsKey(profileId))
                return false;

            var plyr = Players[profileId];
            if(plyr == null)
                return false;

            plyr.ProcessModuleReplicationPatch(packet);
           
            return true;
        }

        void ReplicateAirdrop(Dictionary<string, object> packet)
        {
            if (!Singleton<SITAirdropsManager>.Instantiated)
                return;

            Logger.LogInfo("--- RAW AIRDROP PACKET ---");
            Logger.LogInfo(packet.SITToJson());

            Singleton<SITAirdropsManager>.Instance.AirdropParameters = packet["model"].ToString().SITParseJson<AirdropParametersModel>();
        }

        void ReplicateAirdropLoot(Dictionary<string, object> packet)
        {
            if (!Singleton<SITAirdropsManager>.Instantiated)
                return;

            Logger.LogInfo("--- RAW AIRDROP-LOOT PACKET ---");
            Logger.LogInfo(packet.SITToJson());

            Singleton<SITAirdropsManager>.Instance.ReceiveBuildLootContainer(
                packet["result"].ToString().SITParseJson<AirdropLootResultModel>(),
                packet["config"].ToString().SITParseJson<AirdropConfigModel>());
        }

        //void ReplicateRaidTimer(Dictionary<string, object> packet)
        //{
        //    SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
        //    if (coopGameComponent == null)
        //        return;

        //    if (SITMatchmaking.IsClient)
        //    {
        //        var sessionTime = new TimeSpan(long.Parse(packet["sessionTime"].ToString()));
        //        Logger.LogDebug($"RaidTimer: Remaining session time {sessionTime.TraderFormat()}");

        //        if (coopGameComponent.LocalGameInstance is CoopSITGame coopGame)
        //        {
        //            var gameTimer = coopGame.GameTimer;
        //            if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
        //            {
        //                if (gameTimer.PastTime.TotalSeconds < 3)
        //                    return;

        //                var timeRemain = gameTimer.PastTime + sessionTime;

        //                if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
        //                    return;

        //                Logger.LogInfo($"RaidTimer: New SessionTime {timeRemain.TraderFormat()}");
        //                gameTimer.ChangeSessionTime(timeRemain);

        //                MainTimerPanel mainTimerPanel = ReflectionHelpers.GetFieldOrPropertyFromInstance<MainTimerPanel>(coopGame.GameUi.TimerPanel, "_mainTimerPanel", false);
        //                if (mainTimerPanel != null)
        //                {
        //                    FieldInfo extractionDateTimeField = ReflectionHelpers.GetFieldFromType(typeof(TimerPanel), "dateTime_0");
        //                    extractionDateTimeField.SetValue(mainTimerPanel, gameTimer.StartDateTime.Value.AddSeconds(timeRemain.TotalSeconds));

        //                    MethodInfo UpdateTimerMI = ReflectionHelpers.GetMethodForType(typeof(MainTimerPanel), "UpdateTimer");
        //                    UpdateTimerMI.Invoke(mainTimerPanel, new object[] { });
        //                }
        //            }
        //        }
        //    }
        //}

        

        void ReplicateArmoredTrainTime(Dictionary<string, object> packet)
        {
            SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
                return;

            if (SITMatchmaking.IsClient)
            {
                DateTime utcTime = new(long.Parse(packet["utcTime"].ToString()));

                if (coopGameComponent.LocalGameInstance is CoopSITGame coopGame)
                {
                    Timer1 gameTimer = coopGame.GameTimer;

                    // Process only after raid began.
                    if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
                    {
                        // Looking for Armored Train, if there is nothing, then we are not on the Reserve or Lighthouse.
                        Locomotive locomotive = FindObjectOfType<Locomotive>();
                        if (locomotive != null)
                        {
                            // The time won't change, if we already have replicated the time, don't override it again.
                            FieldInfo departField = ReflectionHelpers.GetFieldFromType(typeof(MovingPlatform), "_depart");
                            if (utcTime == (DateTime)departField.GetValue(locomotive))
                                return;

                            locomotive.Init(utcTime);
                        }
                    }
                }
            }
        }
    }
}
