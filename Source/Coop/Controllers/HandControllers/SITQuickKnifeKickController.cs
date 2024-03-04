using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EFT.Player;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public class SITQuickKnifeKickController : QuickKnifeKickController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITQuickKnifeKickController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            ConsoleScreen.Log($"{this.GetType().Name}:{nameof(Spawn)}");
            BepInLogger.LogDebug($"{nameof(SITQuickKnifeKickController)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITQuickKnifeKickController)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITQuickKnifeKickController)}:{nameof(Drop)}");
        }

        public override void Loot(bool p)
        {
            base.Loot(p);
        }

        public override void Pickup(bool p)
        {
            base.Pickup(p);
        }
    }
}
