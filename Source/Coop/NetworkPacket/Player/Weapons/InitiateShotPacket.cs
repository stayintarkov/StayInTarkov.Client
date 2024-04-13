/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */

using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Controllers.HandControllers;
using StayInTarkov.Coop.Players;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class InitiateShotPacket : BasePlayerPacket
    {
        public bool IsPrimaryActive { get; set; }

        public int AmmoAfterShot { get; set; }

        public EShotType ShotType { get; set; }

        public Vector3 ShotPosition { get; set; }

        public Vector3 ShotDirection { get; set; }

        public Vector3 FireportPosition { get; set; }

        public int ChamberIndex { get; set; }

        public float Overheat { get; set; }

        public bool UnderbarrelShot { get; set; }

        static ManualLogSource Logger;

        static InitiateShotPacket()
        {
            InitiateShotPacket.Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(InitiateShotPacket));
        }

        public InitiateShotPacket()
        {
        }

        public InitiateShotPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(InitiateShotPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(IsPrimaryActive);
            writer.Write(AmmoAfterShot);
            writer.Write((byte)ShotType);
            Vector3Utils.Serialize(writer, ShotPosition);
            Vector3Utils.Serialize(writer, ShotDirection);
            Vector3Utils.Serialize(writer, FireportPosition);
            writer.Write(ChamberIndex);
            writer.Write(Overheat);
            writer.Write(UnderbarrelShot);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));  
            ReadHeaderAndProfileId(reader);
            IsPrimaryActive = reader.ReadBoolean();
            AmmoAfterShot = reader.ReadInt32();
            ShotType = (EShotType)reader.ReadByte();
            ShotPosition = Vector3Utils.Deserialize(reader);
            ShotDirection = Vector3Utils.Deserialize(reader);
            FireportPosition = Vector3Utils.Deserialize(reader);
            ChamberIndex = reader.ReadInt32();
            Overheat = reader.ReadSingle();
            UnderbarrelShot = reader.ReadBoolean();
            return this;

        }

        protected override void Process(CoopPlayerClient client)
        {
            if(client.HandsController is SITFirearmControllerClient firearmControllerClient)
            {
                GetBulletToFire(client, firearmControllerClient.Weapon, out var ammoToFire);
                if (ammoToFire == null)
                {
                    StayInTarkovHelperConstants.Logger.LogError($"Unable to find Ammo for {firearmControllerClient.Weapon.Name}");
                    return;
                }
                //ammoToFire.IsUsed = true;
                switch (ShotType)
                {
                    case EShotType.DryFire:
                        firearmControllerClient.DryShot(ChamberIndex, UnderbarrelShot);

                        // If I am DryFiring a used bullet in the Chamber, remove it
                        if (firearmControllerClient.Weapon.HasChambers && firearmControllerClient.Weapon.Chambers[0].ContainedItem != null)
                            firearmControllerClient.Weapon.Chambers[0].RemoveItem().OrElse(elseValue: false);

                        break;
                    case EShotType.Misfire:
                    case EShotType.Feed:
                    case EShotType.JamedShot:
                    case EShotType.SoftSlidedShot:
                    case EShotType.HardSlidedShot:
                        firearmControllerClient.Weapon.MalfState.MalfunctionedAmmo = ammoToFire;
                        WeaponPrefab component = firearmControllerClient.ControllerGameObject.GetComponent<WeaponPrefab>();
                        if (component != null)
                        {
                            component.InitMalfunctionState(firearmControllerClient.Weapon, hasPlayer: false, malfunctionKnown: false, out var _);
                        }
                        switch (ShotType)
                        {
                            case EShotType.Misfire:
                                firearmControllerClient.Weapon.MalfState.State = Weapon.EMalfunctionState.Misfire;
                                break;
                            case EShotType.Feed:
                                firearmControllerClient.Weapon.MalfState.State = Weapon.EMalfunctionState.Feed;
                                break;
                            case EShotType.JamedShot:
                                firearmControllerClient.Weapon.MalfState.State = Weapon.EMalfunctionState.Jam;
                                break;
                            case EShotType.SoftSlidedShot:
                                firearmControllerClient.Weapon.MalfState.State = Weapon.EMalfunctionState.SoftSlide;
                                break;
                            case EShotType.HardSlidedShot:
                                firearmControllerClient.Weapon.MalfState.State = Weapon.EMalfunctionState.HardSlide;
                                break;
                        }
                        break;
                    case EShotType.RegularShot:
                        firearmControllerClient.InitiateShot(firearmControllerClient.Weapon, ammoToFire, ShotPosition, ShotDirection, FireportPosition, ChamberIndex, Overheat);
                        firearmControllerClient.PlaySounds(firearmControllerClient.WeaponSoundPlayer, ammoToFire, ShotPosition, ShotDirection, false);
                        //firearmControllerClient.FirearmsAnimator.SetFire(fire: true);

                        if (firearmControllerClient.Weapon.IsBoltCatch && firearmControllerClient.Weapon.ChamberAmmoCount == 0 && firearmControllerClient.Weapon.GetCurrentMagazineCount() == 0 && !firearmControllerClient.Weapon.ManualBoltCatch)
                        {
                            firearmControllerClient.FirearmsAnimator.SetBoltCatch(active: true);
                        }
                        else if (firearmControllerClient.Weapon.IsBoltCatch && !firearmControllerClient.Weapon.ManualBoltCatch && firearmControllerClient.FirearmsAnimator.GetBoltCatch())
                        {
                            firearmControllerClient.FirearmsAnimator.SetBoltCatch(active: false);
                        }
                        if (firearmControllerClient.Weapon.BoltAction)
                        {
                            firearmControllerClient.FirearmsAnimator.SetBoltActionReload(boltActionReload: true);
                            firearmControllerClient.FirearmsAnimator.SetFire(fire: true);
                            firearmControllerClient.StartCoroutine(DisableBoltActionAnim(firearmControllerClient));
                        }

                        if (firearmControllerClient.Weapon.GetCurrentMagazine() is CylinderMagazineClass cylindermag)
                        {
                            cylindermag.IncrementCamoraIndex();
                            firearmControllerClient.FirearmsAnimator.SetCamoraIndex(cylindermag.CurrentCamoraIndex);
                        }

                        if (ammoToFire.AmmoTemplate.IsLightAndSoundShot)
                        {
                            firearmControllerClient.method_56(ShotPosition, ShotDirection);
                            firearmControllerClient.LightAndSoundShot(ShotPosition, ShotDirection, ammoToFire.AmmoTemplate);
                        }

                        //if (firearmControllerClient.Weapon.HasChambers && firearmControllerClient.Weapon.Chambers[0].ContainedItem != null)
                         //   firearmControllerClient.Weapon.Chambers[0].RemoveItem().OrElse(elseValue: false);

                        //ammoToFire.IsUsed = true;

                        break;
                    default:
                        break;
                }


                ammoToFire = null;
            }
        }

        private IEnumerator DisableBoltActionAnim(SITFirearmControllerClient client)
        {
            yield return new WaitForSeconds(0.5f);
            client.FirearmsAnimator.SetBoltActionReload(boltActionReload: false);
            client.FirearmsAnimator.SetFire(fire: false);
        }

        private void GetBulletToFire(CoopPlayerClient client, Weapon weapon_0, out BulletClass ammoToFire)
        {
            if (weapon_0.GetCurrentMagazine() is CylinderMagazineClass cylindermag)
            {
                ammoToFire = cylindermag.GetFirstAmmo(singleFireMode: false);
                // Is Used?
                ammoToFire.IsUsed = true;
                return;
            }

            var pic = ItemFinder.GetPlayerInventoryController(client);

            // Find the Ammo in the Chamber
            Slot[] chambers = weapon_0.Chambers;
            ammoToFire = (weapon_0.HasChambers ? chambers[0] : null)?.ContainedItem as BulletClass;
            // ammoToFire 
            if (ammoToFire != null)
            {
                Logger.LogDebug($"Used {ammoToFire} in Chamber");
                return;
            }

            // If there is no ammo in the chamber. Get it from the magazine.
            if (ammoToFire == null)
            {
                MagazineClass currentMagazine = weapon_0.GetCurrentMagazine();
                if (currentMagazine == null)
                    return;

                //if (currentMagazine.IsAmmoCompatible(chambers))
                //{
                //    ammoToFire = (BulletClass)currentMagazine.Cartridges.PopTo(pic, new SlotItemAddress(chambers[0])).Value.Item;

                //    Logger.LogDebug($"Popped {ammoToFire} to {new SlotItemAddress(chambers[0])}");
                //}
                //else
                //{
                    ammoToFire = (BulletClass)currentMagazine.Cartridges.PopToNowhere(pic).Value.Item;

                //    Logger.LogDebug($"Popped {ammoToFire} to nowhere");
                //}

                
            }

            // Is Used?
            ammoToFire.IsUsed = true;
            
            if (ammoToFire == null)
            {
                Logger.LogError($"Unable to find Ammo to Fire for {client.ProfileId} weapon {weapon_0}");
            }
        }

       
    }
}
