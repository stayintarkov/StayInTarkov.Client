using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player
{

    /// <summary>
    /// </summary>
    internal class Player_Move_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Move";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
          EFT.Player __instance,
          ref UnityEngine.Vector2 direction
           )
        {
            // don't run this if we dont have a "player"
            if (__instance == null)
                return false;

            var player = __instance;

            // If this player is a Client drone, then don't run this method
            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            if (prc.IsClientDrone)
                return false;

            direction.x = (float)Math.Round(direction.x, 3);
            direction.y = (float)Math.Round(direction.y, 3);

            return true;

        }

        public static Dictionary<string, Vector2> LastDirections { get; } = new();

        //static PlayerMovePacket playerMovePacket = new();//null;// new(player.ProfileId);

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           ref UnityEngine.Vector2 direction
            )
        {
            var player = __instance;
            // don't run this if we dont have a "player"
            if (__instance == null)
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                Logger.LogError($"Unable to find PRC on {player.ProfileId}");
                return;
            }

            if (prc.IsClientDrone)
                return;

            PlayerMovePacket playerMovePacket = new(player.ProfileId, player.Position.x, player.Position.y, player.Position.z, direction.x, direction.y, player.MovementContext.CharacterMovementSpeed);
            
            if (!PlayerMovePackets.ContainsKey(player.ProfileId))
            {
                PlayerMovePackets.Add(player.ProfileId, playerMovePacket);
                AkiBackendCommunication.Instance.SendDataToPool(playerMovePacket.Serialize());
            }
            else
            {
                var lastMovePacket = (PlayerMovePackets[player.ProfileId]);

                if (
                    lastMovePacket.dX.ApproxEquals(direction.x)
                    && lastMovePacket.dY.ApproxEquals(direction.y)
                    && lastMovePacket.pX.ApproxEquals(player.Position.x)
                    && lastMovePacket.pY.ApproxEquals(player.Position.y)
                    && lastMovePacket.pZ.ApproxEquals(player.Position.z)
                    )
                    return;

                lastMovePacket.Dispose();
                lastMovePacket = null;
                PlayerMovePackets[player.ProfileId] = playerMovePacket;
                AkiBackendCommunication.Instance.SendDataToPool(playerMovePacket.Serialize());
            }
        }

        private static Dictionary<string, PlayerMovePacket> PlayerMovePackets { get; } = new Dictionary<string, PlayerMovePacket>();

        //PlayerMovePacket ReplicatedPMP = null;// new(player.ProfileId);

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            // Player Moves happen too often for this check. This would be a large mem leak!
            //if (HasProcessed(this.GetType(), player, dict))
            //    return;

            PlayerMovePacket ReplicatedPMP = new(null, 0, 0, 0, 0, 0, 0);
            ReplicatedPMP = ReplicatedPMP.DeserializePacketSIT(dict["data"].ToString());
            ReplicatedMove(player, ReplicatedPMP);

            ReplicatedPMP = null;
            dict = null;
        }

        public void ReplicatedMove(EFT.Player player, PlayerMovePacket playerMovePacket)
        {
            if (!player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
                return;

            if (!playerReplicatedComponent.IsClientDrone)
                return;

            ReplicatedMove(player
                , new ReceivedPlayerMoveStruct(
                    playerMovePacket.pX
                    , playerMovePacket.pY
                    , playerMovePacket.pZ
                    , playerMovePacket.dX
                    , playerMovePacket.dY
                    , playerMovePacket.spd
                    ));
            playerMovePacket.Dispose();
            playerMovePacket = null;

            //if (playerMovePacket.pX != 0 && playerMovePacket.pY != 0 && playerMovePacket.pZ != 0)
            //{
            //    //player.Teleport(new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ));
            //    var ReplicatedPosition = new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ);
            //    var replicationDistance = Vector3.Distance(ReplicatedPosition, player.Position);
            //    if (replicationDistance >= 3)
            //    {
            //        player.Teleport(ReplicatedPosition, true);
            //    }
            //    else
            //    {
            //        player.Position = Vector3.Lerp(player.Position, ReplicatedPosition, Time.deltaTime * 7);
            //    }
            //}

            //UnityEngine.Vector2 direction = new(playerMovePacket.dX, playerMovePacket.dY);
            //float spd = playerMovePacket.spd;

            //playerReplicatedComponent.ReplicatedMovementSpeed = spd;
            //playerReplicatedComponent.ReplicatedDirection = null;

            //player.InputDirection = direction;
            //player.MovementContext.MovementDirection = direction;

            //player.MovementContext.CharacterMovementSpeed = spd;

            ////player.CurrentManagedState.Move(direction);

            //playerReplicatedComponent.ReplicatedDirection = direction;

            //playerReplicatedComponent = null;

        }

        public void ReplicatedMove(EFT.Player player, ReceivedPlayerMoveStruct playerMoveStruct)
        {
            PlayerReplicatedComponent playerReplicatedComponent = null;
            if (player.GetComponent<PlayerReplicatedComponent>() == null)
                return;

            playerReplicatedComponent = player.GetComponent<PlayerReplicatedComponent>();

            if (!playerReplicatedComponent.IsClientDrone)
                return;

            if (playerMoveStruct.pX != 0 && playerMoveStruct.pY != 0 && playerMoveStruct.pZ != 0)
            {
                var ReplicatedPosition = new Vector3(playerMoveStruct.pX, playerMoveStruct.pY, playerMoveStruct.pZ);
                var replicationDistance = Vector3.Distance(ReplicatedPosition, player.Position);
                if (replicationDistance >= 3)
                {
                    Logger.LogDebug($"{player.Profile.Nickname} replication distance {replicationDistance} is further than 3, Teleporting");
                    player.Teleport(ReplicatedPosition, true);
                }
                else
                {
                    player.Position = Vector3.Lerp(player.Position, ReplicatedPosition, Time.deltaTime * 7);
                }
            }

            UnityEngine.Vector2 direction = new(playerMoveStruct.dX, playerMoveStruct.dY);
            float spd = playerMoveStruct.spd;

            //player.InputDirection = direction;
            //player.MovementContext.MovementDirection = direction;
            //player.MovementContext.PlayerAnimatorSetMovementDirection(direction);

            //playerReplicatedComponent.ReplicatedMovementSpeed = spd;
            //player.MovementContext.CharacterMovementSpeed = spd;

            player.CurrentManagedState.Move(direction);

            playerReplicatedComponent = null;
        }
    }
}
