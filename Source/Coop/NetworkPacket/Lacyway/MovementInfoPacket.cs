using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using EFT;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct MovementInfoPacket
    {
        public EPlayerState EPlayerState { get; set; }
        public Vector3 Position { get; set; }
        public Vector2 Direction { get; set; }
        public int AnimatorStateIndex { get; set; }
        public int DiscreteDirection { get; set; }
        public float PoseLevel { get; set; }
        public float CharacterMovementSpeed { get; set; }
        public float Tilt { get; set; }
        public int Step { get; set; }
        public int BlindFire { get; set; }
        public bool SoftSurface { get; set; }
        public Physical.Stamina0 Stamina { get; set; }
        public PacketItemInteraction InteractWithDoorPacket { get; set; }
        public LootInteractionPacket LootInteractionPacket { get; set; }
        public StationaryWeaponPacket StationaryWeaponPacket { get; set; }
        public PlantItemPacket PlantItemPacket { get; set; }
        public Vector3 HeadRotation { get; set; }
        public bool IsGrounded { get; set; }
        public bool FullPositionSync { get; set; }
        public float AimRotation { get; set; }
        public float FallHeight { get; set; }
        public float FallTime { get; set; }
        public float FootRotation { get; set; }
        public float JumpHeight { get; internal set; }
        public float MaxSpeed { get; internal set; }
        public Vector2 MovementDirection { get; internal set; }
        public EPhysicalCondition PhysicalCondition { get; internal set; }
        public EPlayerPose Pose { get; internal set; }
        public float SprintSpeed { get; internal set; }
        public EPlayerState State { get; internal set; }
        public Vector3 Velocity { get; internal set; }
    }
}
