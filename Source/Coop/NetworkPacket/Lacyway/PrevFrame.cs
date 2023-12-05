using BepInEx.Logging;
using EFT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// GClass2179

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct PrevFrame
    {
        private static ManualLogSource Logger;
        public PrevFrame()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("PrevFrame");
        }

        public MovementInfoPacket MovementInfoPacket { get; set; }
        public HelmetLightPacket HelmetLightPacket { get; set; }
        public List<ICommand> Commands { get; set; } = [];

        public void ClearFrame()
        {
            Commands.Clear();
        }

        public static byte[] Serialize(List<ICommand> commands, MovementInfoPacket movementInfoPacket)
        {
            // AimRotation and FootRotation

            Writer writer = new();

            // Position
            writer.WriteVector3(movementInfoPacket.Position);
            //Head Rotation
            GStruct85 headStruct = new(GClass1048.ScaleFloatToByte(movementInfoPacket.HeadRotation.x, -50f, 20f), GClass1048.ScaleFloatToByte(movementInfoPacket.HeadRotation.y, -40f, 40f));
            headStruct.Serialize(writer);
            // MovementDir
            GClass1048.ScaleToVector2Byte(movementInfoPacket.MovementDirection, -1f, 1f).Serialize(writer);
            // Velocity
            GClass1048.ScaleToVector3Short(movementInfoPacket.Velocity, -25f, 25f).Serialize(writer);
            // AnimatorStateIndex
            writer.WriteByte((byte)movementInfoPacket.AnimatorStateIndex);
            // Tilt
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.Tilt, -5f, 5f));
            // Step 
            writer.WriteByte(GClass1048.ScaleIntToByte(movementInfoPacket.Step, -1, 1));
            // PoseLevel
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.PoseLevel, 0f, 1f));
            // BlindFire
            writer.WriteByte(GClass1048.ScaleIntToByte(movementInfoPacket.BlindFire, -1, 1));
            // State
            writer.WriteByte((byte)movementInfoPacket.State);
            // PhysicalCondition
            writer.WriteByte((byte)movementInfoPacket.PhysicalCondition);
            // CharacterMovementSpeed
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.CharacterMovementSpeed, 0f, 1f));
            // SprintSpeed
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.SprintSpeed, 0f, 1f));
            // MaxSpeed
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.MaxSpeed, 0f, 1f));
            // WeaponOverlap
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.WeaponOverlap, 0f, 1f));
            // IsGrounded
            byte value = 0;
            GClass1048.AttachToMask(ref value, 0, movementInfoPacket.IsGrounded);
            writer.WriteByte(value);
            // JumpHeight
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.JumpHeight, -10f, 10f));
            // FallHeight
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.FallHeight, 0f, 10f));
            // FallTime
            writer.WriteByte(GClass1048.ScaleFloatToByte(movementInfoPacket.FallTime, 0f, 5f));
            // Pose
            writer.WriteByte((byte)movementInfoPacket.Pose);
            // Aim & Foot Rotation to BodyDir
            writer.WriteFloat(movementInfoPacket.FootRotation);
            writer.WriteShort(GClass1048.ScaleFloatToShort(movementInfoPacket.AimRotation, -90f, 90f));

            var commands2 = GClass2179.CreateInstance();
            commands.CopyTo(commands2);
            commands2.Serialize(writer);

            return writer.ToArray();
        }

        public static GStruct256 Deserialize(byte[] package)
        {
            // AimRotation and FootRotation

            if (package == null)
            {
                Logger.LogError("PrevFrame::Deserialize package was null");
                return default;
            }

            GClass1035 reader = new(package);

            GStruct256 nextModel = new();

            // Position
            nextModel.Movement.BodyPosition = reader.ReadVector3();
            // HeadRotation
            GStruct85 gstruct = default;
            gstruct.Deserialize(reader);
            nextModel.Movement.HeadRotation = new Vector2(GClass1048.ScaleByteToFloat(gstruct.x, -50f, 20f), GClass1048.ScaleByteToFloat(gstruct.y, -40f, 40f));
            // MovementDirection
            GStruct85 movDirStruct = default;
            movDirStruct.Deserialize(reader);
            nextModel.Movement.MovementDirection = GClass1048.ScaleFromVector2Byte(movDirStruct, -1f, 1f);
            // Velocity
            GStruct88 velStruct = default;
            velStruct.Deserialize(reader);
            nextModel.Movement.Velocity = GClass1048.ScaleFromVector3Short(velStruct, -25f, 25f);
            // AnimatorStateIndex
            nextModel.Movement.StateAnimatorIndex = (int)reader.ReadByte();
            // Tilt
            nextModel.Movement.Tilt = GClass1048.ScaleByteToFloat(reader.ReadByte(), -5f, 5f);
            // Step
            nextModel.Movement.Step = GClass1048.ScaleByteToInt(reader.ReadByte(), -1, 1);
            // PoseLevel
            nextModel.Movement.PoseLevel = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            // BlindFire
            nextModel.Movement.BlindFire = GClass1048.ScaleByteToInt(reader.ReadByte(), -1, 1);
            // State
            nextModel.Movement.State = (EPlayerState)reader.ReadByte();
            // PhysicalCondition
            nextModel.Movement.PhysicalCondition = (EPhysicalCondition)reader.ReadByte();
            // MovementSpeed
            nextModel.Movement.MovementSpeed = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            // SprintSpeed
            nextModel.Movement.SprintSpeed = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            // MaxSpeed
            nextModel.Movement.MaxSpeed = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            // InHandsObjectOverlap
            nextModel.Movement.InHandsObjectOverlap = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            // IsGrounded
            byte mask = reader.ReadByte();
            nextModel.Movement.IsGrounded = GClass1048.AttachedToMask(mask, 0);
            // JumpHeight
            nextModel.Movement.JumpHeight = GClass1048.ScaleByteToFloat(reader.ReadByte(), -10f, 10f);
            // FallHeight
            nextModel.Movement.FallHeight = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 10f);
            // FallTime
            nextModel.Movement.FallTime = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 5f);
            // Pose
            nextModel.Movement.Pose = (EPlayerPose)reader.ReadByte();
            // Rotation
            Vector2 rotation = new(reader.ReadFloat(), GClass1048.ScaleShortToFloat(reader.ReadShort(), -90f, 90f));
            // AimRotation
            nextModel.Movement.AimRotation = rotation.y;
            // FootRotation
            nextModel.Movement.FootRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);

            var commands = GClass2179.CreateInstance();
            commands.Deserialize(reader);

            nextModel.Commands = commands.ToArray();
            nextModel.CommandsCount = commands.Count();

            return nextModel;
        }
    }
}
