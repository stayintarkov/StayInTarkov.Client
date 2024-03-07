using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons.Grenade;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public sealed class SITGrenadeController : EFT.Player.GrenadeController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITFirearmController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{this.GetType().Name}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{this.GetType().Name}:{nameof(Execute)}:{operation}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{this.GetType().Name}:{nameof(Drop)}");
        }

        public override void HighThrow()
        {
            base.HighThrow();
            GrenadeHighThrowPacket packet = new GrenadeHighThrowPacket(_player.ProfileId, _player.Rotation);
            GameClient.SendData(packet.Serialize());
        }

        public override void LowThrow()
        {
            base.LowThrow();
            GrenadeLowThrowPacket packet = new GrenadeLowThrowPacket(_player.ProfileId, _player.Rotation);
            GameClient.SendData(packet.Serialize());
        }

        public override void PullRingForHighThrow()
        {
            base.PullRingForHighThrow();
            GrenadePullRingForHighThrowPacket packet = new GrenadePullRingForHighThrowPacket(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
        }

        public override void PullRingForLowThrow()
        {
            base.PullRingForLowThrow();
            GrenadePullRingForLowThrowPacket packet = new GrenadePullRingForLowThrowPacket(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
        }

    }
}
