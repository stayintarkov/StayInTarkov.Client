using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public class SITFirearmController : EFT.Player.FirearmController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITFirearmController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            ConsoleScreen.Log($"{this.GetType().Name}:{nameof(Spawn)}");
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Drop)}");
        }

        public override void QuickReloadMag(MagazineClass magazine, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(QuickReloadMag)}");
            if (CanStartReload())
            {
                base.QuickReloadMag(magazine, callback);
                QuickReloadMagPacket quickReloadMagPacket = new QuickReloadMagPacket();
                quickReloadMagPacket.ProfileId = _player.ProfileId;
                quickReloadMagPacket.ItemId = magazine.Id;
                GameClient.SendData(quickReloadMagPacket.Serialize());
            }
        }

        public override void ReloadMag(MagazineClass magazine, GridItemAddress gridItemAddress, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadMag(magazine, gridItemAddress, callback);
                ReloadMagPacket reloadMagPacket = new ReloadMagPacket(_player.ProfileId, magazine.Id, gridItemAddress);
                reloadMagPacket.ProfileId = _player.ProfileId;
                reloadMagPacket.ItemId = magazine.Id;
                reloadMagPacket.GridItemAddress = gridItemAddress;
                GameClient.SendData(reloadMagPacket.Serialize());
            }
        }

        public override void ReloadWithAmmo(AmmoPack ammoPack, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadWithAmmo(ammoPack, callback);
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadWithAmmoPacket packet = new (_player.ProfileId, ammoPack.GetReloadingAmmoIds());
                GameClient.SendData(packet.Serialize());
            }
        }

        public override bool CheckAmmo()
        {
            StayInTarkov.Coop.NetworkPacket.Player.Weapons.CheckAmmoPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.CheckAmmo();
        }

        public override bool CheckChamber()
        {
            StayInTarkov.Coop.NetworkPacket.Player.Weapons.CheckChamberPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.CheckChamber();
        }

        public override bool CheckFireMode()
        {
            StayInTarkov.Coop.NetworkPacket.Player.Weapons.CheckFireModePacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.CheckFireMode();
        }

        public override void BlindFire(int b)
        {
            base.BlindFire(b);
        }

        public override void ChangeAimingMode()
        {
            ChangeAimingModePacket packet = new(_player.ProfileId, Item.AimIndex.Value);
            GameClient.SendData(packet.Serialize());
            base.ChangeAimingMode();
        }

        public override bool ChangeFireMode(Weapon.EFireMode fireMode)
        {
            ChangeFireModePacket packet = new(_player.ProfileId, fireMode);
            GameClient.SendData(packet.Serialize());
            return base.ChangeFireMode(fireMode);
        }

        public override bool ExamineWeapon()
        {
            return base.ExamineWeapon();
        }

        public override void Loot(bool p)
        {
            base.Loot(p);
        }

        public override void Pickup(bool p)
        {
            base.Pickup(p);
        }

        public override void SetLightsState(LightsStates[] lightsStates, bool force = false)
        {
            base.SetLightsState(lightsStates, force);
        }

        public override void SetScopeMode(ScopeStates[] scopeStates)
        {
            base.SetScopeMode(scopeStates);
        }

        public override void SetTriggerPressed(bool pressed)
        {
            TriggerPressedPacket packet = new TriggerPressedPacket(_player.ProfileId);
            packet.Pressed = pressed;
            packet.RotationX = _player.Rotation.x;
            packet.RotationY = _player.Rotation.y;
            GameClient.SendData(packet.Serialize());

            ((CoopPlayer)_player).TriggerPressed = pressed;


            base.SetTriggerPressed(pressed);

        }

        public override void ToggleAim()
        {
            ToggleAimPacket packet = new ToggleAimPacket(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            base.ToggleAim();
        }

        public override bool ToggleLauncher()
        {
            return base.ToggleLauncher();
        }
    }
}
