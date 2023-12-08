using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;

namespace StayInTarkov.AI.PMCLogic.RushSpawn
{
    /// <summary>
    /// Created by: Paulov
    /// </summary>
    public class PMCRushSpawnLayer : CustomLayer
    {
        protected ManualLogSource Logger;
        protected bool isActive = false;

        public PMCRushSpawnLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Logger.LogInfo($"Added PMCRushSpawnLayer to {botOwner.name}");
        }

        public override string GetName()
        {
            return "PMCRushSpawn";
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

            // If we are not Pmc. Then don't run this.
            if (BotOwner.Side != EPlayerSide.Usec && BotOwner.Side != EPlayerSide.Bear)
            {
                return false;
            }

            isActive = true;
            return true;
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(PMCRushSpawnLogic), "PMCRushSpawn");
        }

        public override bool IsCurrentActionEnding()
        {
            // We only have one action, so it's never ending
            return false;
        }
    }
}
