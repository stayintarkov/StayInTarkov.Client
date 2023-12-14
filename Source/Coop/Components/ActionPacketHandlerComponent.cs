using Aki.Custom.Airdrops;
using Aki.Custom.Airdrops.Models;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.World;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Components
{
    public class ActionPacketHandlerComponent : MonoBehaviour
    {
        public BlockingCollection<Dictionary<string, object>> ActionPackets { get; } = new(9999);
        public BlockingCollection<Dictionary<string, object>> ActionPacketsMovement { get; private set; } = new(9999);
        public BlockingCollection<Dictionary<string, object>> ActionPacketsDamage { get; private set; } = new(9999);
        public ConcurrentDictionary<string, EFT.Player> Players => CoopGameComponent.Players;
        public ManualLogSource Logger { get; private set; }

        private List<string> RemovedFromAIPlayers = new();

        private CoopGame CoopGame { get; } = (CoopGame)Singleton<AbstractGame>.Instance;

        private CoopGameComponent CoopGameComponent { get; set; }

        void Awake()
        {
            // ----------------------------------------------------
            // Create a BepInEx Logger for ActionPacketHandlerComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource("ActionPacketHandlerComponent");
            Logger.LogDebug("Awake");

            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
            ActionPacketsMovement = new();
        }

        void Start()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
            ActionPacketsMovement = new();
        }

        void Update()
        {
            ProcessActionPackets();
        }


        public static ActionPacketHandlerComponent GetThisComponent()
        {
            if (CoopPatches.CoopGameComponentParent == null)
                return null;

            if (CoopPatches.CoopGameComponentParent.TryGetComponent<ActionPacketHandlerComponent>(out var component))
                return component;

            return null;
        }

        private void ProcessActionPackets()
        {
            if (CoopGameComponent == null)
            {
                if (CoopPatches.CoopGameComponentParent != null)
                {
                    CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
                    if (CoopGameComponent == null)
                        return;
                }
            }

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (ActionPackets == null)
                return;

            if (Players == null)
                return;

            if (ActionPackets.Count > 0)
            {
                Stopwatch stopwatchActionPackets = Stopwatch.StartNew();
                while (ActionPackets.TryTake(out var result))
                {
                    Stopwatch stopwatchActionPacket = Stopwatch.StartNew();
                    if (!ProcessLastActionDataPacket(result))
                    {
                        //ActionPackets.Add(result);
                        continue;
                    }

                    if (stopwatchActionPacket.ElapsedMilliseconds > 1)
                        Logger.LogDebug($"ActionPacket {result["m"]} took {stopwatchActionPacket.ElapsedMilliseconds}ms to process!");
                }
                if (stopwatchActionPackets.ElapsedMilliseconds > 1)
                    Logger.LogDebug($"ActionPackets took {stopwatchActionPackets.ElapsedMilliseconds}ms to process!");
            }

            if (ActionPacketsMovement != null && ActionPacketsMovement.Count > 0)
            {
                Stopwatch stopwatchActionPacketsMovement = Stopwatch.StartNew();
                while (ActionPacketsMovement.TryTake(out var result))
                {
                    if (!ProcessLastActionDataPacket(result))
                    {
                        //ActionPacketsMovement.Add(result);
                        continue;
                    }
                }
                if (stopwatchActionPacketsMovement.ElapsedMilliseconds > 1)
                {
                    Logger.LogDebug($"ActionPacketsMovement took {stopwatchActionPacketsMovement.ElapsedMilliseconds}ms to process!");
                }
            }


            if (ActionPacketsDamage != null && ActionPacketsDamage.Count > 0)
            {
                Stopwatch stopwatchActionPacketsDamage = Stopwatch.StartNew();
                while (ActionPacketsDamage.TryTake(out var packet))
                {
                    if (!packet.ContainsKey("profileId"))
                        continue;

                    var profileId = packet["profileId"].ToString();

                    // The person is missing. Lets add this back until they exist
                    if (!CoopGameComponent.Players.ContainsKey(profileId))
                    {
                        //ActionPacketsDamage.Add(packet);
                        continue;
                    }

                    var playerKVP = CoopGameComponent.Players[profileId];
                    if (playerKVP == null)
                        continue;

                    var coopPlayer = (CoopPlayer)playerKVP;
                    coopPlayer.ReceiveDamageFromServer(packet);
                }
                if (stopwatchActionPacketsDamage.ElapsedMilliseconds > 1)
                {
                    Logger.LogDebug($"ActionPacketsDamage took {stopwatchActionPacketsDamage.ElapsedMilliseconds}ms to process!");
                }
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

            if (!packet.ContainsKey("profileId"))
                return false;

            var profileId = packet["profileId"].ToString();

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

            var prc = plyr.GetComponent<PlayerReplicatedComponent>();
            if (prc == null)
                return false;
                
            prc.ProcessPacket(packet);
            return true;
        }

        async Task WaitForPlayerAndProcessPacket(string profileId, Dictionary<string, object> packet)
        {
            // Start the timer.
            var startTime = DateTime.Now;
            var maxWaitTime = TimeSpan.FromMinutes(2);

            while (true)
            {
                // Check if maximum wait time has been reached.
                if (DateTime.Now - startTime > maxWaitTime)
                {
                    Logger.LogError($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: WaitForPlayerAndProcessPacket waited for {maxWaitTime.TotalMinutes} minutes, but player {profileId} still did not exist after timeout period.");
                    return;
                }

                if (Players == null)
                    continue;

                var registeredPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;

                // If the player now exists, process the packet and end the thread.
                if (Players.Any(x => x.Key == profileId) || registeredPlayers.Any(x => x.Profile.ProfileId == profileId))
                {
                    // Logger.LogDebug($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: WaitForPlayerAndProcessPacket waited for {(DateTime.Now - startTime).TotalSeconds}s");
                    ProcessPlayerPacket(packet);
                    return;
                }

                // Wait for a short period before checking again.
                await Task.Delay(1000);
            }
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

        void ReplicateRaidTimer(Dictionary<string, object> packet)
        {
            CoopGameComponent coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
                return;

            if (MatchmakerAcceptPatches.IsClient)
            {
                var sessionTime = new TimeSpan(long.Parse(packet["sessionTime"].ToString()));
                Logger.LogInfo($"RaidTimer: Remaining session time {sessionTime.TraderFormat()}");

                if (coopGameComponent.LocalGameInstance is CoopGame coopGame)
                {
                    var gameTimer = coopGame.GameTimer;
                    if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
                    {
                        if (gameTimer.PastTime.TotalSeconds < 3)
                            return;

                        var timeRemain = gameTimer.PastTime + sessionTime;

                        if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
                            return;

                        Logger.LogInfo($"RaidTimer: New SessionTime {timeRemain.TraderFormat()}");
                        gameTimer.ChangeSessionTime(timeRemain);

                        // FIXME: Giving SetTime() with empty exfil point arrays has a known bug that may cause client game crashes!
                        coopGame.GameUi.TimerPanel.SetTime(gameTimer.StartDateTime.Value, coopGame.Profile_0.Info.Side, gameTimer.SessionSeconds(), new EFT.Interactive.ExfiltrationPoint[] { });
                    }
                }
            }
        }

        void ReplicateTimeAndWeather(Dictionary<string, object> packet)
        {
            CoopGameComponent coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
                return;

            if (MatchmakerAcceptPatches.IsClient)
            {
                Logger.LogDebug(packet.ToJson());

                var gameDateTime = new DateTime(long.Parse(packet["GameDateTime"].ToString()));
                if (coopGameComponent.LocalGameInstance is CoopGame coopGame && coopGame.GameDateTime != null)
                    coopGame.GameDateTime.Reset(gameDateTime);

                var weatherController = EFT.Weather.WeatherController.Instance;
                if (weatherController != null)
                {
                    var weatherDebug = weatherController.WeatherDebug;
                    if (weatherDebug != null)
                    {
                        weatherDebug.Enabled = true;

                        weatherDebug.CloudDensity = float.Parse(packet["CloudDensity"].ToString());
                        weatherDebug.Fog = float.Parse(packet["Fog"].ToString());
                        weatherDebug.LightningThunderProbability = float.Parse(packet["LightningThunderProbability"].ToString());
                        weatherDebug.Rain = float.Parse(packet["Rain"].ToString());
                        weatherDebug.Temperature = float.Parse(packet["Temperature"].ToString());
                        weatherDebug.TopWindDirection = new(float.Parse(packet["TopWindDirection.x"].ToString()), float.Parse(packet["TopWindDirection.y"].ToString()));

                        Vector2 windDirection = new(float.Parse(packet["WindDirection.x"].ToString()), float.Parse(packet["WindDirection.y"].ToString()));

                        // working dog sh*t, if you are the programmer, DON'T EVER DO THIS! - dounai2333
                        static bool BothPositive(float f1, float f2) => f1 > 0 && f2 > 0;
                        static bool BothNegative(float f1, float f2) => f1 < 0 && f2 < 0;
                        static bool VectorIsSameQuadrant(Vector2 v1, Vector2 v2, out int flag)
                        {
                            flag = 0;
                            if (v1.x != 0 && v1.y != 0 && v2.x != 0 && v2.y != 0)
                            {
                                if ((BothPositive(v1.x, v2.x) && BothPositive(v1.y, v2.y))
                                || (BothNegative(v1.x, v2.x) && BothNegative(v1.y, v2.y))
                                || (BothPositive(v1.x, v2.x) && BothNegative(v1.y, v2.y))
                                || (BothNegative(v1.x, v2.x) && BothPositive(v1.y, v2.y)))
                                {
                                    flag = 1;
                                    return true;
                                }
                            }
                            else
                            {
                                if (v1.x != 0 && v2.x != 0)
                                {
                                    if (BothPositive(v1.x, v2.x) || BothNegative(v1.x, v2.x))
                                    {
                                        flag = 1;
                                        return true;
                                    }
                                }
                                else if (v1.y != 0 && v2.y != 0)
                                {
                                    if (BothPositive(v1.y, v2.y) || BothNegative(v1.y, v2.y))
                                    {
                                        flag = 2;
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }

                        for (int i = 1; i < WeatherClass.WindDirections.Count(); i++)
                        {
                            Vector2 direction = WeatherClass.WindDirections[i];
                            if (VectorIsSameQuadrant(windDirection, direction, out int flag))
                            {
                                weatherDebug.WindDirection = (EFT.Weather.WeatherDebug.Direction)i;
                                weatherDebug.WindMagnitude = flag switch
                                {
                                    1 => windDirection.x / direction.x,
                                    2 => windDirection.y / direction.y,
                                    _ => weatherDebug.WindMagnitude
                                };
                                break;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError("TimeAndWeather: WeatherDebug is null!");
                    }
                }
                else
                {
                    Logger.LogError("TimeAndWeather: WeatherController is null!");
                }
            }
        }
    }
}
