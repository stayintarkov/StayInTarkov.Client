using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket
{
    public sealed class PlayerStatePacket : BasePlayerPacket, INetSerializable
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
            PlayerHealth = playerHealth;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(PositionX);
            writer.Write(PositionY);
            writer.Write(PositionZ);
            writer.Write(RotationX);
            writer.Write(RotationY);
            writer.Write(HeadRotationX);
            writer.Write(HeadRotationY);
            writer.Write(MovementDirectionX);
            writer.Write(MovementDirectionY);
            writer.Write(State.ToString());
            writer.Write(Tilt);
            writer.Write(Step);
            writer.Write(AnimatorStateIndex);
            writer.Write(CharacterMovementSpeed);
            writer.Write(IsProne);
            writer.Write(PoseLevel);
            writer.Write(IsSprinting);
            writer.Write(InputDirectionX);
            writer.Write(InputDirectionY);

            // Has PlayerHealth packet
            writer.Write(PlayerHealth != null);
            if (PlayerHealth != null)
                writer.WriteLengthPrefixedBytes(PlayerHealth.Serialize());

            return ms.ToArray();

        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();

            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return this;

            PositionX = reader.ReadSingle();
            PositionY = reader.ReadSingle();
            PositionZ = reader.ReadSingle();
            RotationX = reader.ReadSingle();
            RotationY = reader.ReadSingle();
            HeadRotationX = reader.ReadSingle();
            HeadRotationY = reader.ReadSingle();
            MovementDirectionX = reader.ReadSingle();
            MovementDirectionY = reader.ReadSingle();
            State = (EPlayerState)Enum.Parse(typeof(EPlayerState), reader.ReadString());
            Tilt = reader.ReadSingle();
            Step = reader.ReadInt32();
            AnimatorStateIndex = reader.ReadInt32();
            CharacterMovementSpeed = reader.ReadSingle();
            IsProne = reader.ReadBoolean();
            PoseLevel = reader.ReadSingle();
            IsSprinting = reader.ReadBoolean();
            InputDirectionX = reader.ReadSingle();
            InputDirectionY = reader.ReadSingle();

            // If has a PlayerHealth packet
            if (reader.ReadBoolean())
            {
                PlayerHealth = new PlayerHealthPacket(ProfileId);
                PlayerHealth = (PlayerHealthPacket)PlayerHealth.Deserialize(reader.ReadLengthPrefixedBytes());
            }

            //StayInTarkovHelperConstants.Logger.LogInfo(this.SITToJson());
            return this;
        }

        /// <summary>
        /// DO NOT AUTODESERIALIZE
        /// </summary>
        /// <param name="serializedPacket"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override ISITPacket AutoDeserialize(byte[] serializedPacket)
        {
            //return base.DeserializePacketSIT(serializedPacket);
            throw new NotImplementedException();    
        }

        void INetSerializable.Serialize(NetDataWriter writer)
        {
            var serializedSIT = Serialize();
            writer.Put(serializedSIT.Length);
            writer.Put(serializedSIT);
        }

        void INetSerializable.Deserialize(NetDataReader reader)
        {
            var length = reader.GetInt();
            byte[] bytes = new byte[length];
            reader.GetBytes(bytes, length);
            Deserialize(bytes);
        }
    }
}
