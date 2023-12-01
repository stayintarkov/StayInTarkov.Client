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

            var serialized = playerMovePacket.Serialize();
            if (serialized == null)
                return;

            if (!PlayerMovePackets.ContainsKey(player.ProfileId))
            {
                PlayerMovePackets.Add(player.ProfileId, playerMovePacket);
                AkiBackendCommunication.Instance.SendDataToPool(serialized);
            }
            else
            {
                var lastMovePacket = (PlayerMovePackets[player.ProfileId]);

                if (
                    lastMovePacket.dX == direction.x
                    && lastMovePacket.dY == direction.y
                    && lastMovePacket.pX == player.Position.x
                    && lastMovePacket.pY == player.Position.y
                    && lastMovePacket.pZ == player.Position.z
                    )
                    return;

                lastMovePacket.Dispose();
                lastMovePacket = null;
                PlayerMovePackets[player.ProfileId] = playerMovePacket;
                AkiBackendCommunication.Instance.SendDataToPool(serialized);
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
            if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
            {
                if (playerReplicatedComponent.IsClientDrone)
                {
                    if (playerMovePacket.pX != 0 && playerMovePacket.pY != 0 && playerMovePacket.pZ != 0)
                    {
                        //player.Teleport(new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ));
                        var ReplicatedPosition = new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ);
                        player.Teleport(ReplicatedPosition, true);
                        player.CurrentManagedState.ChangeSpeed(playerMovePacket.spd);
                        player.Move(ReplicatedPosition);
                    }
                }
            }

            if (CoopGameComponent.TestControllerUseMe != null)
            {
                ToSnapshot toSnapshot = new()
                {
                    State = player.MovementContext.CurrentState.Name,
                    StateAnimatorIndex = player.CurrentAnimatorStateIndex,
                    PoseLevel = player.MovementContext.SmoothedPoseLevel,
                    MovementSpeed = player.MovementContext.ClampSpeed(player.MovementContext.SmoothedCharacterMovementSpeed),
                    Tilt = player.MovementContext.SmoothedTilt,
                    Step = player.MovementContext.Step,
                    BlindFire = player.MovementContext.BlindFire,
                    HeadRotation = player.HeadRotation,
                    MovementDirection = player.MovementContext.MovementDirection,
                    Velocity = player.MovementContext.Velocity,
                    BodyPosition = player.Position,
                    Pose = player.Pose,
                    BodyRotation = player.Rotation,
                    AimRotation = Mathf.Lerp(player.Rotation.y, player.Rotation.y, 1f),
                    FallHeight = player.MovementContext.FallHeight,
                    FallTime = player.MovementContext.FreefallTime,
                    IsGrounded = player.MovementContext.IsGrounded,
                    PhysicalCondition = player.MovementContext.PhysicalCondition,
                    SprintSpeed = player.MovementContext.SprintSpeed,
                    JumpHeight = player.MovementContext.JumpHeight,
                    MaxSpeed = player.MovementContext.MaxSpeed
                };

                CoopGameComponent.TestControllerUseMe.Interpolator.Add(toSnapshot);
            }
        }

        public void ReplicatedMove(EFT.Player player, ReceivedPlayerMoveStruct playerMoveStruct)
        {
            if (!player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
                return;

            if (!playerReplicatedComponent.IsClientDrone)
                return;

            if (playerMoveStruct.pX != 0 && playerMoveStruct.pY != 0 && playerMoveStruct.pZ != 0)
            {
                //player.Teleport(new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ));
                var ReplicatedPosition = new Vector3(playerMoveStruct.pX, playerMoveStruct.pY, playerMoveStruct.pZ);
                player.Teleport(ReplicatedPosition, true);
                player.CurrentManagedState.ChangeSpeed(playerMoveStruct.spd);
                player.Move(ReplicatedPosition);

            }

            if (CoopGameComponent.TestControllerUseMe != null)
            {
                ToSnapshot toSnapshot = new()
                {
                    State = player.MovementContext.CurrentState.Name,
                    StateAnimatorIndex = player.CurrentAnimatorStateIndex,
                    PoseLevel = player.MovementContext.SmoothedPoseLevel,
                    MovementSpeed = player.MovementContext.ClampSpeed(player.MovementContext.SmoothedCharacterMovementSpeed),
                    Tilt = player.MovementContext.SmoothedTilt,
                    Step = player.MovementContext.Step,
                    BlindFire = player.MovementContext.BlindFire,
                    HeadRotation = player.HeadRotation,
                    MovementDirection = player.MovementContext.MovementDirection,
                    Velocity = player.MovementContext.Velocity,
                    BodyPosition = player.Position,
                    Pose = player.Pose,
                    BodyRotation = player.Rotation,
                    AimRotation = Mathf.Lerp(player.Rotation.y, player.Rotation.y, 1f),
                    FallHeight = player.MovementContext.FallHeight,
                    FallTime = player.MovementContext.FreefallTime,
                    IsGrounded = player.MovementContext.IsGrounded,
                    PhysicalCondition = player.MovementContext.PhysicalCondition,
                    SprintSpeed = player.MovementContext.SprintSpeed,
                    JumpHeight = player.MovementContext.JumpHeight,
                    MaxSpeed = player.MovementContext.MaxSpeed
                };

                CoopGameComponent.TestControllerUseMe.Interpolator.Add(toSnapshot);
            }
        }
    }
}
