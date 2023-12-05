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
        public Vector3 Position { get; set; }
        public Vector2 Direction { get; set; }
        public int AnimatorStateIndex { get; set; }
        public float PoseLevel { get; set; }
        public float CharacterMovementSpeed { get; set; }
        public float Tilt { get; set; }
        public int Step { get; set; }
        public int BlindFire { get; set; }
        public Physical.Stamina0 Stamina { get; set; }
        public Vector3 HeadRotation { get; set; }
        public bool IsGrounded { get; set; }
        public float AimRotation { get; set; }
        public float FallHeight { get; set; }
        public float FallTime { get; set; }
        public float FootRotation { get; set; }
        public float JumpHeight { get; set; }
        public float MaxSpeed { get; set; }
        public Vector2 MovementDirection { get; set; }
        public EPhysicalCondition PhysicalCondition { get; set; }
        public EPlayerPose Pose { get; set; }
        public float SprintSpeed { get; set; }
        public EPlayerState State { get; set; }
        public Vector3 Velocity { get; set; }
        public float WeaponOverlap { get; set; }
    }
}
