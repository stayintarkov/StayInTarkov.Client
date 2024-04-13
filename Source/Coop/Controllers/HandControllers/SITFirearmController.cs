using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public class SITFirearmController : EFT.Player.FirearmController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITFirearmController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Execute)}:{operation}");
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

        public override void ReloadBarrels(AmmoPack ammoPack, GridItemAddress placeToPutContainedAmmoMagazine, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadBarrels(ammoPack, placeToPutContainedAmmoMagazine, callback);
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadBarrelsPacket packet = new(_player.ProfileId, ammoPack.GetReloadingAmmoIds(), placeToPutContainedAmmoMagazine);
                GameClient.SendData(packet.Serialize());
            }
        }

        public override void ReloadCylinderMagazine(AmmoPack ammoPack, Callback callback, bool quickReload = false)
        {
            if (CanStartReload())
            {
                base.ReloadCylinderMagazine(ammoPack, callback, quickReload);
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadCylinderMagazinePacket packet = new(_player.ProfileId, ammoPack.GetReloadingAmmoIds(), quickReload);
                GameClient.SendData(packet.Serialize());
            }
        }

        public override void ReloadGrenadeLauncher(AmmoPack foundItem, Callback callback)
        {
            if (!Blindfire)
            {
                if (CanStartReload())
                {
                    CurrentOperation.ReloadGrenadeLauncher(foundItem, callback);
                    StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadGrenadeLauncherPacket packet = new(_player.ProfileId, foundItem.GetReloadingAmmoIds());
                    GameClient.SendData(packet.Serialize());
                }
                else
                {
                    callback?.Fail("Cant StartReload");
                }
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
            CheckFireModePacket packet = new(_player.ProfileId);
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
            ExamineWeaponPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.ExamineWeapon();
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

        public override void SetInventoryOpened(bool opened)
        {
            SetInventoryOpenedPacket packet = new(_player.ProfileId, opened);
            GameClient.SendData(packet.Serialize());
            base.SetInventoryOpened(opened);
        }

        public override void SetLightsState(LightsStates[] lightsStates, bool force = false)
        {
            LightStatesPacket packet = new();
            packet.ProfileId = _player.ProfileId;
            packet.LightStates = lightsStates;
            GameClient.SendData(packet.Serialize());
            base.SetLightsState(lightsStates, force);
        }

        public override void SetScopeMode(ScopeStates[] scopeStates)
        {
            ScopeStatesPacket packet = new();
            packet.ProfileId = _player.ProfileId;
            packet.ScopeStates = scopeStates;
            GameClient.SendData(packet.Serialize());

            base.SetScopeMode(scopeStates);
        }

        //public override void SetTriggerPressed(bool pressed)
        //{
        //    TriggerPressedPacket packet = new TriggerPressedPacket(_player.ProfileId);
        //    packet.Pressed = pressed;
        //    packet.RotationX = _player.Rotation.x;
        //    packet.RotationY = _player.Rotation.y;

        //    // TODO: After release, sync amount of bullets used
        //    if (!pressed)
        //    {
        //    }

        //    GameClient.SendData(packet.Serialize());

        //    ((CoopPlayer)_player).TriggerPressed = pressed;

        //    base.SetTriggerPressed(pressed);

        //}

        public override void InitiateShot(IWeapon weapon, BulletClass ammo, Vector3 shotPosition, Vector3 shotDirection, Vector3 fireportPosition, int chamberIndex, float overheat)
        {
            base.InitiateShot(weapon, ammo, shotPosition, shotDirection, fireportPosition, chamberIndex, overheat);

            //if (((Weapon)weapon).HasChambers && ((Weapon)weapon).Chambers[0].ContainedItem != null && ((Weapon)weapon).Chambers[0].ContainedItem == ammo && ammo.IsUsed)
            //    ((Weapon)weapon).Chambers[0].RemoveItem().OrElse(elseValue: false);

            EShotType shotType = EShotType.Unknown;
            switch (weapon.MalfState.State)
            {
                case Weapon.EMalfunctionState.None:
                    shotType = EShotType.RegularShot;
                    break;
                case Weapon.EMalfunctionState.Misfire:
                    shotType = EShotType.Misfire;
                    break;
                case Weapon.EMalfunctionState.Jam:
                    shotType = EShotType.JamedShot;
                    break;
                case Weapon.EMalfunctionState.HardSlide:
                    shotType = EShotType.HardSlidedShot;
                    break;
                case Weapon.EMalfunctionState.SoftSlide:
                    shotType = EShotType.SoftSlidedShot;
                    break;
                case Weapon.EMalfunctionState.Feed:
                    shotType = EShotType.Feed;
                    break;
            }
            InitiateShotPacket initiateShotPacket = new InitiateShotPacket(_player.ProfileId);
            initiateShotPacket.IsPrimaryActive = (weapon == base.Item);
            initiateShotPacket.ShotType = shotType;
            initiateShotPacket.ShotPosition = shotPosition;
            initiateShotPacket.ShotDirection = shotDirection;
            initiateShotPacket.FireportPosition = fireportPosition;
            initiateShotPacket.AmmoAfterShot = weapon.GetCurrentMagazineCount();
            initiateShotPacket.ChamberIndex = chamberIndex;
            initiateShotPacket.Overheat = overheat;
            initiateShotPacket.UnderbarrelShot = weapon.IsUnderbarrelWeapon;
            GameClient.SendData(initiateShotPacket.Serialize());
        }

        public override void IEventsConsumerOnFiringBullet()
        {
            Logger.LogInfo(nameof(IEventsConsumerOnFiringBullet));
            base.IEventsConsumerOnFiringBullet();
        }

        public override void ToggleAim()
        {
            ToggleAimPacket packet = new ToggleAimPacket(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            base.ToggleAim();
        }

        public override bool ToggleLauncher()
        {
            ToggleLauncherPacket packet = new ToggleLauncherPacket(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            return base.ToggleLauncher();
        }

        public override void CreateFlareShot(BulletClass flareItem, Vector3 shotPosition, Vector3 forward)
        {
            var createFlareShotPacket = new CreateFlareShotPacket(_player.ProfileId, shotPosition, forward, flareItem.TemplateId);
            GameClient.SendData(createFlareShotPacket.Serialize());

            base.CreateFlareShot(flareItem, shotPosition, forward);
        }

    }
}
