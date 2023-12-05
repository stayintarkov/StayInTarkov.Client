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

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, "ThrowItem", false, true);

        [PatchPostfix]
        public static void PostPatch(Item item, EFT.Player ___player_0)
        {
            var coopPlayer = ___player_0 as CoopPlayer;
            var observedPlayer = Singleton<GameWorld>.Instance.GetObservedPlayerByProfileID(coopPlayer.ProfileId);

            if (observedPlayer != null)
            {
                Vector3 position = observedPlayer.PlayerColliderPointOnCenterAxis(0.65f) + observedPlayer.Velocity * Time.deltaTime;
                Quaternion rotation = observedPlayer.PlayerBones.WeaponRoot.rotation * Quaternion.Euler(90f, 0f, 0f);
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