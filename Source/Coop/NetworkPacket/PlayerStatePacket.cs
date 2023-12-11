using EFT;
using System;
using System.IO;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerStatePacket : BasePlayerPacket, IDisposable
    {
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; }
        public Vector3 HeadRotation { get; set; }
        public Vector2 MovementDirection { get; set; }
        public EPlayerState State { get; set; }
        public float Tilt { get; set; }
        public int Step { get; set; }
        public int AnimatorStateIndex { get; set; }
        public float CharacterMovementSpeed { get; set; }
        public bool IsProne { get; set; }
        public float PoseLevel { get; set; }
        public bool IsSprinting { get; set; }
        public Physical.PhysicalStamina Stamina { get; set; }
        public Vector2 InputDirection { get; set; }

        public PlayerStatePacket(string profileId, Vector3 position, Vector2 rotation, Vector2 headRotation, Vector2 movementDirection,
            EPlayerState state, float tilt, int step, int animatorStateIndex, float characterMovementSpeed,
            bool isProne, float poseLevel, bool isSprinting, Physical.PhysicalStamina stamina, Vector2 inputDirection) : base(profileId, "ApplyState")
        {
            ProfileId = profileId;
            Position = position;
            Rotation = rotation;
            HeadRotation = headRotation;
            MovementDirection = movementDirection;
            State = state;
            Tilt = tilt;
            Step = step;
            AnimatorStateIndex = animatorStateIndex;
            CharacterMovementSpeed = characterMovementSpeed;
            IsProne = isProne;
            PoseLevel = poseLevel;
            IsSprinting = isSprinting;
            Stamina = stamina;
            InputDirection = inputDirection;
        }

        public static byte[] SerializeState(PlayerStatePacket playerStatePacket)
        {
            GClass1040 writer = new();

            writer.WriteString(playerStatePacket.ProfileId);
            writer.WriteVector3(playerStatePacket.Position);
            writer.WriteVector2(playerStatePacket.Rotation);
            GStruct85 headStruct = new(GClass1048.ScaleFloatToByte(playerStatePacket.HeadRotation.x, -50f, 20f), GClass1048.ScaleFloatToByte(playerStatePacket.HeadRotation.y, -40f, 40));
            headStruct.Serialize(writer);
            GClass1048.ScaleToVector2Byte(playerStatePacket.MovementDirection, -1f, 1f).Serialize(writer);
            writer.WriteByte((byte)playerStatePacket.State);
            writer.WriteByte(GClass1048.ScaleFloatToByte(playerStatePacket.Tilt, -5f, 5f));
            writer.WriteByte(GClass1048.ScaleIntToByte(playerStatePacket.Step, -1, 1));
            writer.WriteByte((byte)playerStatePacket.AnimatorStateIndex);
            writer.WriteByte(GClass1048.ScaleFloatToByte(playerStatePacket.CharacterMovementSpeed, 0f, 1f));
            writer.WriteBool(playerStatePacket.IsProne);
            writer.WriteByte(GClass1048.ScaleFloatToByte(playerStatePacket.PoseLevel, 0f, 1f));
            writer.WriteBool(playerStatePacket.IsSprinting);
            writer.WriteBool(playerStatePacket.Stamina.StaminaExhausted);
            writer.WriteBool(playerStatePacket.Stamina.OxygenExhausted);
            writer.WriteBool(playerStatePacket.Stamina.HandsExhausted);
            writer.WriteVector2(playerStatePacket.InputDirection);

            return writer.ToArray();
        }

        public static PlayerStatePacket DeserializeState(byte[] serializedPacket)
        {
            if (serializedPacket == null)
            {
                EFT.UI.ConsoleScreen.LogError("PrevFrame::Deserialize package was null");
                return default;
            }

            GClass1035 reader = new(serializedPacket);

            var profileId = reader.ReadString();
            var position = reader.ReadVector3();
            var rotation = reader.ReadVector2();
            GStruct85 headRotStruct = default;
            headRotStruct.Deserialize(reader);
            var headRotation = new Vector2(GClass1048.ScaleByteToFloat(headRotStruct.x, -50f, 20f), GClass1048.ScaleByteToFloat(headRotStruct.y, -40f, 40f));
            GStruct85 movDirStruct = default;
            movDirStruct.Deserialize(reader);
            var movDir = GClass1048.ScaleFromVector2Byte(movDirStruct, -1f, 1f);
            var state = (EPlayerState)reader.ReadByte();
            var tilt = GClass1048.ScaleByteToFloat(reader.ReadByte(), -5f, 5f);
            var step = GClass1048.ScaleByteToInt(reader.ReadByte(), -1, 1);
            var animatorStateIndex = (int)reader.ReadByte();
            var characterMovementSpeed = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            var isProne = reader.ReadBool();
            var poseLevel = GClass1048.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            var isSprinting = reader.ReadBool();
            var stamina = new Physical.PhysicalStamina()
            {
                StaminaExhausted = reader.ReadBool(),
                OxygenExhausted = reader.ReadBool(),
                HandsExhausted = reader.ReadBool(),
            };
            var inputDirection = reader.ReadVector2();

            return new PlayerStatePacket(profileId, position, rotation, headRotation, movDir, state, tilt, step, animatorStateIndex, characterMovementSpeed, isProne, poseLevel, isSprinting, stamina, inputDirection);
        }

        public void Dispose()
        {
            ProfileId = null;
            //StayInTarkovHelperConstants.Logger.LogDebug("PlayerMovePacket.Dispose");
        }
    }
}