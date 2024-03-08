using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons.Knife;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public sealed class SITKnifeController : EFT.Player.KnifeController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITKnifeController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{nameof(SITKnifeController)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITKnifeController)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITKnifeController)}:{nameof(Drop)}");
        }

        public override void BrakeCombo()
        {
            base.BrakeCombo();
        }

        public override void ContinueCombo()
        {
            base.ContinueCombo();
        }

        public override void ExamineWeapon()
        {
            ExamineWeaponPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            base.ExamineWeapon();
        }

        public override bool MakeAlternativeKick()
        {
            MakeKnifeAlternateKickPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.MakeAlternativeKick();
        }

        public override bool MakeKnifeKick()
        {
            MakeKnifeKickPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.MakeKnifeKick();
        }

        public override void Loot(bool p)
        {
            LootPacket packet = new(_player.ProfileId, p);
            GameClient.SendData(packet.Serialize());
            base.Loot(p);
        }

        public override void Pickup(bool p)
        {
            PickupPacket packet = new(_player.ProfileId, p);
            GameClient.SendData(packet.Serialize());
            base.Pickup(p);
        }
    }
}
