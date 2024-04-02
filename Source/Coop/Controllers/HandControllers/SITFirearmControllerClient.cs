using BepInEx.Logging;
using Comfort.Common;
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
    public sealed class SITFirearmControllerClient : EFT.Player.FirearmController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITFirearmControllerClient));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmControllerClient)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmControllerClient)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITFirearmControllerClient)}:{nameof(Drop)}");
        }

        public override void QuickReloadMag(MagazineClass magazine, Callback callback)
        {
            if (CanStartReload())
            {
                base.QuickReloadMag(magazine, callback);
            }
        }

        public override void ReloadMag(MagazineClass magazine, GridItemAddress gridItemAddress, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadMag(magazine, gridItemAddress, callback);
            }
        }

        public override void ReloadWithAmmo(AmmoPack ammoPack, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadWithAmmo(ammoPack, callback);
            }
        }

        public override bool CheckAmmo()
        {
            return base.CheckAmmo();
        }

        public override bool CheckChamber()
        {
            return base.CheckChamber();
        }

        public override bool CheckFireMode()
        {
            return base.CheckFireMode();
        }

        public override void BlindFire(int b)
        {
            base.BlindFire(b);
        }

        public override void ChangeAimingMode()
        {
            base.ChangeAimingMode();
        }

        public override bool ChangeFireMode(Weapon.EFireMode fireMode)
        {
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
            base.SetTriggerPressed(pressed);
        }

        public override void ToggleAim()
        {
            base.ToggleAim();
        }

        public override bool ToggleLauncher()
        {
            return base.ToggleLauncher();
        }

        public void PlaySounds(WeaponSoundPlayer weaponSoundPlayer, BulletClass ammo, Vector3 shotPosition, Vector3 shotDirection, bool multiShot)
        {
            if (Item.FireMode.FireMode != Weapon.EFireMode.burst || Item.FireMode.BurstShotsCount != 2 || IsBirstOf2Start || Item.ChamberAmmoCount <= 0)
            {
                float pitchMult = method_55();
                weaponSoundPlayer.FireBullet(ammo, shotPosition, shotDirection.normalized, pitchMult, Malfunction, multiShot, IsBirstOf2Start);
            }
        }

        public override void SetInventoryOpened(bool opened)
        {
            base.SetInventoryOpened(opened);
        }
    }
}
