using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.NetworkPacket.Player.Inventory;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Networking;
using System;
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

        public override bool CanExecute(IAbstractOperation operation)
        {
            return base.CanExecute(operation);
        }

        public override void Execute(IAbstractOperation operation, Callback callback)
        {
            //Apply the same checks BSG does before invoking DropBackpackOperationInvoke
            if (!method_18(operation))
            {
                IOneItemOperation ProperOperation = TryGetIOneItemOperation(operation);

                if (ProperOperation != null && IsAnimatedSlot(ProperOperation))
                {
                    BepInLogger.LogInfo($"{nameof(SITFirearmController)}:Attempt to quickly replicate a backpack drop");

                    PlayerInventoryDropBackpackPacket packet = new();
                    packet.ProfileId = _player.ProfileId;

                    GameClient.SendData(packet.Serialize());

                    //Do not send the current operation to the inventory server as the packet we just sent already handles this.
                    AbstractInventoryOperation AbstractOperation = (AbstractInventoryOperation)operation;
                    var CoopInventoryController = ((CoopInventoryController)ItemFinder.GetPlayerInventoryController(_player));
                    CoopInventoryController.IgnoreOperation(AbstractOperation.Id);
                }

            }

            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Execute)}:{operation}");
            base.Execute(operation, callback);
        }

        private IOneItemOperation TryGetIOneItemOperation(IAbstractOperation operation)
        {
            if (!(operation is IOneItemOperation ItemOperation))
                return null;

            return ItemOperation;
        }

        //Replicate IsAnimatedSlot as this method is not available to us.
        private bool IsAnimatedSlot(IOneItemOperation ItemOperation)
        {
            try
            {
                if (ItemOperation == null)
                    return false;

                var PlyInventoryController = (InventoryControllerClass)ItemFinder.GetPlayerInventoryController(_player);

                //We either picked something up or something is messed up, dont continue in either case.
                if (PlyInventoryController == null || ItemOperation.From1 == null)
                    return false;

                //I'm not doing BSG's dumb looping code here (At least that's what it looks like in ILSpy) for only one item in an array
                if (PlyInventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack) == ItemOperation.From1.Container)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                BepInLogger.LogError($"{nameof(SITFirearmController)}:{nameof(IsAnimatedSlot)}:{ex}");
                return false;
            }
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
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.QuickReloadMagPacket quickReloadMagPacket = new();
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
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadMagPacket reloadMagPacket = new(_player.ProfileId, magazine.Id, gridItemAddress);
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
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadWithAmmoPacket packet = new(_player.ProfileId, ammoPack.GetReloadingAmmoIds());
                GameClient.SendData(packet.Serialize());

                base.ReloadWithAmmo(ammoPack, callback);
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
            InitiateShotPacket initiateShotPacket = new(_player.ProfileId);
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
            ToggleAimPacket packet = new(_player.ProfileId);
            GameClient.SendData(packet.Serialize());
            base.ToggleAim();
        }

        public override bool ToggleLauncher()
        {
            ToggleLauncherPacket packet = new(_player.ProfileId);
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
