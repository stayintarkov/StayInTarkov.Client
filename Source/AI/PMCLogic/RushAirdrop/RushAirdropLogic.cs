using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;
using UnityEngine.AI;

namespace StayInTarkov.AI.PMCLogic.RushAirdrop
{
    /// <summary>
    /// Created by: Paulov
    /// </summary>
    internal class RushAirdropLogic : CustomLogic
    {
        protected ManualLogSource Logger;
        Vector3? targetPos = null;
        NavMeshPath navMeshPath;
        private BotSteering baseSteeringLogic;

        public RushAirdropLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            navMeshPath = new NavMeshPath();
            baseSteeringLogic = new BotSteering(bot);
        }

        public override void Start()
        {
            // When we start roaming, disable the bot patroller, since we're in control now
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            // When we're done this layer, re-enable the patroller
            BotOwner.PatrollingData.Unpause();

            // Clear targetPos, so we pick a new one next time we are active
            targetPos = null;
        }

        public override void Update()
        {
            // Look where you're going
            BotOwner.SetPose(1f);
            BotOwner.Steering.LookToMovingDirection();
            BotOwner.SetTargetMoveSpeed(1f);

            // If we have a target position, and we're already there, clear it
            if (targetPos != null && (targetPos.Value - BotOwner.Position).sqrMagnitude < 4f)
            {
                Logger.LogDebug($"{BotOwner.name} reach destination");
                targetPos = null;
            }

            if (!Singleton<SITAirdropsManager>.Instantiated)
                return;

            if (Singleton<SITAirdropsManager>.Instance.AirdropParameters == null)
                return;

            if (Singleton<SITAirdropsManager>.Instance.AirdropBox == null)
                return;

            if (!Singleton<SITAirdropsManager>.Instance.AirdropBox.enabled)
                return;

            if (Singleton<SITAirdropsManager>.Instance.AirdropBox.gameObject == null)
                return;

            //var airdropBoxPosition = Singleton<SITAirdropsManager>.Instance.AirdropBox.gameObject.transform.position;

            //if (NavMesh.SamplePosition(airdropBoxPosition, out var navHit, 100.0f, NavMesh.AllAreas))
            //{
            //    //if (NavMesh.CalculatePath(BotOwner.Position, navHit.position, NavMesh.AllAreas, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            //    //{
            //    //    targetPos = navHit.position;
            //    //    BotOwner.GoToPoint(targetPos.Value, true, -1f, false, true, true);
            //    //    Logger.LogDebug($"{BotOwner.name} going to {targetPos.Value}");
            //    //}
            //}

            if (targetPos == null)
            {
                //Logger.LogError($"Unable to find a location for {BotOwner.name}");
                Stop();
            }

            BotOwner.DoorOpener.Update();
        }


    }
}
