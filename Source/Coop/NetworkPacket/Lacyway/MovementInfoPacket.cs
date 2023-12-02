using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct MovementInfoPacket
    {
        public EPlayerState EPlayerState { get; set; }
        public Vector3 Position { get; set; }
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
        public Vector3 SurfaceNormal { get; set; }
        public Vector3 PlayerSurfaceUpAlignNormal { get; set; }
        public bool FullPositionSync { get; set; }
    }
}
