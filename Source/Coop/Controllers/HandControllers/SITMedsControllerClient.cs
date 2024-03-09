using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using System;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public class SITMedsControllerClient : EFT.Player.MedsController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITMedsControllerClient));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{nameof(SITMedsControllerClient)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITMedsControllerClient)}:{nameof(Execute)}:{operation}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITMedsControllerClient)}:{nameof(Drop)}");
        }
    }
}
