//using BepInEx.Logging;
//using DrakiaXYZ.BigBrain.Brains;
//using EFT;
//using System;
//using UnityEngine;
//using UnityEngine.AI;

//namespace DrakiaXYZ.Waypoints.BrainLogic
//{
//// Note: We only include this in debug builds for now, because we're not shipping BigBrain
//#if DEBUG
//    internal class RoamingLogic : CustomLogic
//    {
//        protected ManualLogSource Logger;
//        Vector3? targetPos = null;
//        float sprintCheckTime;
//        NavMeshPath navMeshPath;
//        private BotSteering baseSteeringLogic;

//        public RoamingLogic(BotOwner bot) : base(bot)
//        {
//            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
//            navMeshPath = new NavMeshPath();
//            baseSteeringLogic = new BotSteering(bot);
//        }

//        public override void Start()
//        {
//            // When we start roaming, disable the bot patroller, since we're in control now
//            BotOwner.PatrollingData.Pause();
//        }

//        public override void Stop()
//        {
//            // When we're done this layer, re-enable the patroller
//            BotOwner.PatrollingData.Unpause();

//            // Clear targetPos, so we pick a new one next time we are active
//            targetPos = null;
//        }

//        public override void Update()
//        {
//            // Look where you're going
//            BotOwner.SetPose(1f);
//            BotOwner.Steering.LookToMovingDirection();
//            BotOwner.SetTargetMoveSpeed(1f);

//            // Alternate between running and walking
//            if (BotOwner.Mover.Sprinting && BotOwner.GetPlayer.Physical.Stamina.NormalValue < 0.3f)
//            {
//                //Logger.LogDebug($"{BotOwner.name} Ending Sprint");
//                BotOwner.GetPlayer.EnableSprint(false);
//            }

//            // Enough stamina to check? See if we're within our time window
//            if (!BotOwner.Mover.Sprinting && BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0.8f)
//            {
//                if (sprintCheckTime < Time.time)
//                {
//                    sprintCheckTime = Time.time + 5f;

//                    // Random chance to sprint
//                    int randomChance = UnityEngine.Random.Range(0, 1000);
//                    //Logger.LogDebug($"{BotOwner.name} Stamina: {BotOwner.GetPlayer.Physical.Stamina.NormalValue}  Random: {randomChance}  Chance: {BotOwner.Settings.FileSettings.Patrol.SPRINT_BETWEEN_CACHED_POINTS}");
//                    if (randomChance < BotOwner.Settings.FileSettings.Patrol.SPRINT_BETWEEN_CACHED_POINTS)
//                    {
//                        //Logger.LogDebug($"{BotOwner.name} Starting Sprint");
//                        BotOwner.GetPlayer.EnableSprint(true);
//                    }
//                }
//            }

//            // If we have a target position, and we're already there, clear it
//            if (targetPos != null && (targetPos.Value - BotOwner.Position).sqrMagnitude < 4f)
//            {
//                Logger.LogDebug($"{BotOwner.name} reach destination");
//                targetPos = null;
//            }

//            // If we don't have a target position yet, pick one
//            int i = 0;
//            while (targetPos == null && i < 10)
//            {
//                Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 100f;
//                randomPos += BotOwner.Position;
//                if (NavMesh.SamplePosition(randomPos, out var navHit, 100f, NavMesh.AllAreas))
//                {
//                    if (NavMesh.CalculatePath(BotOwner.Position, navHit.position, NavMesh.AllAreas, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
//                    {
//                        targetPos = navHit.position;
//                        BotOwner.GoToPoint(targetPos.Value, true, -1f, false, true, true);
//                        Logger.LogDebug($"{BotOwner.name} going to {targetPos.Value}");
//                    }
//                }

//                i++;
//            }

//            if (targetPos == null)
//            {
//                Logger.LogError($"Unable to find a location for {BotOwner.name}");
//            }

//            BotOwner.DoorOpener.Update();

//            //baseSteeringLogic.Update(BotOwner);
//        }


//    }
//#endif
//}
