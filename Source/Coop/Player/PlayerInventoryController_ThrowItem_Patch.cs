using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StayInTarkov.Coop.Player
{
    internal class PlayerInventoryController_ThrowItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.PlayerInventoryController);

        public override string MethodName => "ThrowItem";

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

        [PatchPostfix]
        public static void PostPatch(ItemController __instance, Item item)
        {
            var coopPlayer = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;

            if (coopPlayer != null)
            {
                Vector3 position = coopPlayer.PlayerColliderPointOnCenterAxis(0.65f) + coopPlayer.Velocity * Time.deltaTime;
                Quaternion rotation = coopPlayer.PlayerBones.WeaponRoot.rotation * Quaternion.Euler(90f, 0f, 0f);
                Vector3 angularVelocity = new(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 2f * Mathf.Sign(Random.Range(-1, 2)));
                var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(item);

                coopPlayer.AddCommand(new GClass2130()
                {
                    AngularVelocity = angularVelocity,
                    Position = position,
                    Rotation = rotation,
                    Velocity = angularVelocity,
                    Item = Components
                });
            }
            else
            {
                Logger.LogError("PlayerInventoryController::ThrowItem CoopPlayer was null!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }
    }
}