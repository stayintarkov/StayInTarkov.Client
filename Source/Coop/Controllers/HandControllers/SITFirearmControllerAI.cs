using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public sealed class SITFirearmControllerAI : SITFirearmController
    {
        public override Vector3 WeaponDirection => _player.LookDirection;

        public override void InitiateShot(IWeapon weapon, BulletClass ammo, Vector3 shotPosition, Vector3 shotDirection, Vector3 fireportPosition, int chamberIndex, float overheat)
        {
            overheat = 0;
            Malfunction = false;
            base.InitiateShot(weapon, ammo, shotPosition, shotDirection, fireportPosition, chamberIndex, overheat);
            overheat = 0;
            Malfunction = false;
        }
    }
}
