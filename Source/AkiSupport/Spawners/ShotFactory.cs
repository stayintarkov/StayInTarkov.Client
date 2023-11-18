using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using System;
using System.Reflection;
using UnityEngine;

namespace SIT.Tarkov.Core.Spawners
{
    public class ShotFactory
    {
        private static BallisticsCalculator ballisticsCalculator;

        private static MethodInfo methodShoot;

        private static Player player;

        private static Weapon weapon;

        private static MethodBase methodCreateShot;

        public static object MakeShot(object ammo, Vector3 shotPosition, Vector3 shotDirection, float speedFactor)
        {
            object obj = methodCreateShot.Invoke(ballisticsCalculator, new object[8]
            {
            ammo,
            shotPosition,
            shotDirection,
            0,
            player,
            weapon,
            speedFactor,
            0
            });
            methodShoot.Invoke(ballisticsCalculator, new object[1] { obj });
            return obj;
        }

        public static object GetBullet(string tid)
        {
            return ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), tid);
        }

        public static void Init(Player player)
        {
            if (ballisticsCalculator == null)
            {
                ballisticsCalculator = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
            }
            if (null == methodShoot)
            {
                methodShoot = ballisticsCalculator.GetType().GetMethod("Shoot");
            }
            if (null == methodCreateShot)
            {
                methodCreateShot = ballisticsCalculator.GetType().GetMethod("CreateShot");
            }
            ShotFactory.player = player;
            if (weapon == null)
            {
                weapon = (Weapon)ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), "5d52cc5ba4b9367408500062");
            }
        }
    }
}
