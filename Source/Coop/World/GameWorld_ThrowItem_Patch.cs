using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.World
{
    internal class GameWorld_ThrowItem_Patch : ModuleReplicationPatch
    {
        public override System.Type InstanceType => typeof(GameWorld);

        public override string MethodName => "ThrowItem";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(GameWorld __instance, Item item, IAIDetails player)
        {
            var coopPlayer = player as CoopPlayer;

            Logger.LogInfo(coopPlayer + " " + player + " ");

            if (coopPlayer != null)
            {
                Vector3 position = player.PlayerColliderPointOnCenterAxis(0.65f) + player.Velocity * Time.deltaTime;
                Quaternion rotation = player.PlayerBones.WeaponRoot.rotation * Quaternion.Euler(90f, 0f, 0f);
                Vector3 angularVelocity = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 2f * Mathf.Sign(Random.Range(-1, 2)));
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
                Logger.LogError("GameWorld_ThrowItem_Patch::PostPatch CoopPlayer was null!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }
    }
}
