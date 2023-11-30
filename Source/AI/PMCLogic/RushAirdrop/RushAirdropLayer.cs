using Aki.Custom.Airdrops;
using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;

namespace StayInTarkov.AI.PMCLogic.RushAirdrop
{
    /// <summary>
    /// Created by: DrakiaXYZ
    /// Link: https://github.com/DrakiaXYZ/SPT-Waypoints/blob/master/BrainLogic/RoamingLayer.cs
    /// </summary>
    internal class RushAirdropLayer : CustomLayer
    {
        protected ManualLogSource Logger;
        protected float nextRoamCheckTime = 0f;
        protected bool isActive = false;

        public RushAirdropLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Logger.LogInfo($"Added RushAirdropLayer to {botOwner.name}");
        }

        public override string GetName()
        {
            return "RushAirdrop";
        }

        public override bool IsActive()
        {
            // If we're not in peace, we can't be roaming, otherwise we die
            if (!BotOwner.Memory.IsPeace)
            {
                return false;
            }

            // If we're active already, then stay active
            if (isActive)
            {
                return true;
            }

            // If it's been long enough, check if we should roam
            if (Time.time > nextRoamCheckTime)
            {
                Logger.LogDebug($"Checking if {BotOwner.name} should RushAirdrop");
                nextRoamCheckTime = Time.time + 10f;

                if (Singleton<SITAirdropsManager>.Instance.AirdropParameters == null)
                    return false;

                if (Singleton<SITAirdropsManager>.Instance.AirdropBox == null)
                    return false;

                if (!Singleton<SITAirdropsManager>.Instance.AirdropBox.enabled)
                    return false;

                if (Singleton<SITAirdropsManager>.Instance.AirdropBox.gameObject == null)
                    return false;

                //if (Random.Range(0, 100) > 30)
                //{
                //    Logger.LogDebug("  RushAirdrop");
                //    isActive = true;
                //    return true;
                //}
            }

            return false;
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(RushAirdropLogic), "RushAirdrop");
        }

        public override bool IsCurrentActionEnding()
        {
            // We only have one action, so it's never ending
            return false;
        }
    }
}
