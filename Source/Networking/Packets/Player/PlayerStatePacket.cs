using LiteNetLib.Utils;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking.Packets
{
    public struct PlayerStatePacket : INetSerializable
    {
        public string ProfileId { get; set; }
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
        public int Blindfire { get; set; }
        public float LinearSpeed { get; set; }

        public PlayerStatePacket()
        {

        }

        public PlayerStatePacket(string profileId, Vector3 position, Vector2 rotation, Vector2 headRotation, Vector2 movementDirection,
            EPlayerState state, float tilt, int step, int animatorStateIndex, float characterMovementSpeed,
            bool isProne, float poseLevel, bool isSprinting, Physical.PhysicalStamina stamina, Vector2 inputDirection, int blindfire, float linearSpeed)
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
            Blindfire = blindfire;
            LinearSpeed = linearSpeed;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            Vector3Utils.Serialize(writer, Position);
            Vector2Utils.Serialize(writer, Rotation);
            Vector2Utils.Serialize(writer, HeadRotation);
            Vector2Utils.Serialize(writer, MovementDirection);
            writer.Put((byte)State);
            writer.Put(GClass1048.ScaleFloatToByte(Tilt, -5f, 5f));
            writer.Put(GClass1048.ScaleIntToByte(Step, -1, 1));
            writer.Put((byte)AnimatorStateIndex);
            writer.Put(GClass1048.ScaleFloatToByte(CharacterMovementSpeed, 0f, 1f));
            writer.Put(IsProne);
            writer.Put(GClass1048.ScaleFloatToByte(PoseLevel, 0f, 1f));
            writer.Put(IsSprinting);
            PhysicalUtils.Serialize(writer, Stamina);
            Vector2Utils.Serialize(writer, InputDirection);
            writer.Put(Blindfire);
            writer.Put(LinearSpeed);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            Position = Vector3Utils.Deserialize(reader);
            Rotation = Vector2Utils.Deserialize(reader);
            HeadRotation = Vector2Utils.Deserialize(reader);
            MovementDirection = Vector2Utils.Deserialize(reader);
            State = (EPlayerState)reader.GetByte();
            Tilt = GClass1048.ScaleByteToFloat(reader.GetByte(), -5f, 5f);
            Step = GClass1048.ScaleByteToInt(reader.GetByte(), -1, 1);
            AnimatorStateIndex = reader.GetByte();
            CharacterMovementSpeed = GClass1048.ScaleByteToFloat(reader.GetByte(), 0f, 1f);
            IsProne = reader.GetBool();
            PoseLevel = GClass1048.ScaleByteToFloat(reader.GetByte(), 0f, 1f);
            IsSprinting = reader.GetBool();
            Stamina = PhysicalUtils.Deserialize(reader);
            InputDirection = Vector2Utils.Deserialize(reader);
            Blindfire = reader.GetInt();
            LinearSpeed = reader.GetFloat();
        }
    }
}