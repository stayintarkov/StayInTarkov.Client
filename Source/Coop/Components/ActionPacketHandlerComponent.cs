using BepInEx.Logging;
using Comfort.Common;
using EFT;
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
        public ConcurrentDictionary<string, ObservedPlayerController> OtherPlayers => CoopGameComponent.OtherPlayers;
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


            //if (ActionPacketsDamage != null && ActionPacketsDamage.Count > 0)
            //{
            //    Stopwatch stopwatchActionPacketsDamage = Stopwatch.StartNew();
            //    while (ActionPacketsDamage.TryTake(out var packet))
            //    {
            //        if (!packet.ContainsKey("profileId"))
            //            continue;

            //        var profileId = packet["profileId"].ToString();

            //        // The person is missing. Lets add this back until they exist
            //        if (!CoopGameComponent.Players.ContainsKey(profileId))
            //        {
            //            //ActionPacketsDamage.Add(packet);
            //            continue;
            //        }

            //        var playerKVP = CoopGameComponent.Players[profileId];
            //        if (playerKVP == null)
            //            continue;

            //        var coopPlayer = (CoopPlayer)playerKVP;
            //        coopPlayer.ReceiveDamageFromServer(packet);
            //    }
            //    if (stopwatchActionPacketsDamage.ElapsedMilliseconds > 1)
            //    {
            //        Logger.LogDebug($"ActionPacketsDamage took {stopwatchActionPacketsDamage.ElapsedMilliseconds}ms to process!");
            //    }
            //}


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
                if (coopPatch is IModuleReplicationWorldPatch imrwp)
                    if (imrwp.MethodName == method)
                    {
                        imrwp.Replicated(ref packet);
                        result = true;
                    }

            if (method == "LootableContainer_Interact")
            {
                LootableContainer_Interact_Patch.Replicated(packet);
                result = true;
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

            if (OtherPlayers == null)
            {
                Logger.LogDebug("Players is Null");
                return false;
            }

            if (OtherPlayers.Count == 0)
            {
                //Logger.LogDebug("Players is Empty");
                return false;
            }

            //var profilePlayers = Players.Where(x => x.Key == profileId && x.Value != null).ToArray();

            // ---------------------------------------------------
            // Causes instance reference errors?
            //var plyr = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(profileId);// Players[profileId];

            // ---------------------------------------------------
            //
            if (!OtherPlayers.ContainsKey(profileId))
                return false;

            var plyr = OtherPlayers[profileId];
            bool processed = false;

            //foreach (var plyr in profilePlayers)
            {
                //if (plyr.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                var prc = plyr.PlayerView.GetComponent<PlayerReplicatedComponent>();
                {
                    prc.ProcessPacket(packet);
                    processed = true;
                }
                //else
                //{
                //    Logger.LogError($"Player {profileId} doesn't have a PlayerReplicatedComponent!");
                //}

                if (packet.ContainsKey("Extracted"))
                {
                    if (CoopGame != null)
                    {
                        //Logger.LogInfo($"Received Extracted ProfileId {packet["profileId"]}");
                        if (!CoopGame.ExtractedPlayers.Contains(packet["profileId"].ToString()))
                            CoopGame.ExtractedPlayers.Add(packet["profileId"].ToString());

                        if (!MatchmakerAcceptPatches.IsClient)
                        {
                            var botController = (BotsController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BaseLocalGame<GamePlayerOwner>), typeof(BotsController)).GetValue(Singleton<AbstractGame>.Instance);
                            if (botController != null)
                            {
                                if (!RemovedFromAIPlayers.Contains(profileId))
                                {
                                    //RemovedFromAIPlayers.Add(profileId);
                                    //Logger.LogDebug("Removing Client Player to Enemy list");
                                    //var botSpawner = (BotSpawner)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BotsController), typeof(BotSpawner)).GetValue(botController);
                                    //botSpawner.DeletePlayer(plyr);
                                }
                            }
                        }
                    }

                    processed = true;
                }
            }

            return processed;
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
    }
}
