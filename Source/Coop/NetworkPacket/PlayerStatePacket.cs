using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerStatePacket : BasePlayerPacket
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; private set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float HeadRotationX { get; set; }
        public float HeadRotationY { get; set; }
        public float HeadRotationZ { get; set; }
        public float MovementDirectionX { get; set; }
        public float MovementDirectionY { get; set; }
        public EPlayerState State { get; set; }
        public float Tilt { get; set; }
        public int Step { get; set; }
        public int AnimatorStateIndex { get; set; }
        public float CharacterMovementSpeed { get; set; }
        public bool IsProne { get; set; }
        public float PoseLevel { get; set; }
        public bool IsSprinting { get; set; }
        public bool HandsExhausted { get; set; }
        public bool OxygenExhausted { get; set; }
        public bool StaminaExhausted { get; set; }
        public float InputDirectionX { get; set; }
        public float InputDirectionY { get; set; }
        public float Energy { get; set; }
        public float Hydration { get; set; }
        public PlayerHealthPacket PlayerHealth { get; set; }

        public PlayerStatePacket() { }

        public PlayerStatePacket(string profileId, Vector3 position, Vector2 rotation, Vector3 headRotation, Vector2 movementDirection,
            EPlayerState state, float tilt, int step, int animatorStateIndex, float characterMovementSpeed,
            bool isProne, float poseLevel, bool isSprinting, Vector2 inputDirection, 
            float energy, float hydration, PlayerHealthPacket playerHealth)
            : base(new string(profileId.ToCharArray()), "PlayerState")
        {
            PositionX = position.x;
            PositionY = position.y;
            PositionZ = position.z;
            RotationX = rotation.x;
            RotationY = rotation.y;
            HeadRotationX = headRotation.x;
            HeadRotationY = headRotation.y;
            MovementDirectionX = movementDirection.x;
            MovementDirectionY = movementDirection.y;
            State = state;
            Tilt = tilt;
            Step = step;
            AnimatorStateIndex = animatorStateIndex;
            CharacterMovementSpeed = characterMovementSpeed;
            IsProne = isProne;
            PoseLevel = poseLevel;
            IsSprinting = isSprinting;
            InputDirectionX = inputDirection.x; 
            InputDirectionY = inputDirection.y;
            Energy = energy;
            Hydration = hydration;
            PlayerHealth = playerHealth;
        }
    }
}
