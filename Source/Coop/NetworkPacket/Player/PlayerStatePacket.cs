using LiteNetLib.Utils;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Coop.Players;
using StayInTarkov.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public sealed class PlayerStatePacket : BasePlayerPacket, INetSerializable
    {
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
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; }
        public Vector3 HeadRotation { get; set; }
        public Vector2 MovementDirection { get; set; }
        public Physical.PhysicalStamina Stamina { get; set; }
        public Vector2 InputDirection { get; set; }
        public int Blindfire { get; set; }
        public float LinearSpeed { get; set; }
        public bool LeftStance { get; set; }

        public PlayerHealthPacket PlayerHealth { get; set; }

        public PlayerStatePacket() : base("", nameof(PlayerStatePacket))
        { 
        }

        public PlayerStatePacket(string profileId, Vector3 position, Vector2 rotation, Vector3 headRotation, Vector2 movementDirection,
            EPlayerState state, float tilt, int step, int animatorStateIndex, float characterMovementSpeed,
            bool isProne, float poseLevel, bool isSprinting, Vector2 inputDirection, bool leftStance
            , PlayerHealthPacket playerHealth, Physical.PhysicalStamina stamina, int blindFire, float linearSpeed)
            : base(new string(profileId.ToCharArray()), nameof(PlayerStatePacket))
        {
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
            InputDirection = inputDirection;
            PlayerHealth = playerHealth;
            Stamina = stamina;
            Blindfire = blindFire;
            LinearSpeed = linearSpeed;
            LeftStance = leftStance;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            Vector3Utils.Serialize(writer, Position);
            Vector2Utils.Serialize(writer, Rotation);
            Vector2Utils.Serialize(writer, HeadRotation);
            Vector2Utils.Serialize(writer, MovementDirection);
            writer.Write(State.ToString());
            writer.Write(BSGNetworkConversionHelpers.ScaleFloatToByte(Tilt, -5f, 5f));
            writer.Write(Step);
            writer.Write(AnimatorStateIndex);
            writer.Write(BSGNetworkConversionHelpers.ScaleFloatToByte(CharacterMovementSpeed, 0f, 1f));
            writer.Write(IsProne);
            writer.Write(PoseLevel);
            writer.Write(IsSprinting);
            PhysicalUtils.Serialize(writer, Stamina);
            Vector2Utils.Serialize(writer, InputDirection);
            writer.Write(LeftStance);
            writer.Write(Blindfire);
            writer.Write(LinearSpeed);
            writer.Write(TimeSerializedBetter);

            // Has PlayerHealth packet
            writer.Write(PlayerHealth != null);
            if (PlayerHealth != null)
                writer.WriteLengthPrefixedBytes(PlayerHealth.Serialize());

            //return Zlib.Compress(ms.ToArray());
            return ms.ToArray();

        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            //bytes = Zlib.DecompressToBytes(bytes);

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Position = Vector3Utils.Deserialize(reader);
            Rotation = Vector2Utils.Deserialize(reader);
            HeadRotation = Vector2Utils.Deserialize(reader);
            MovementDirection = Vector2Utils.Deserialize(reader);
            State = (EPlayerState)Enum.Parse(typeof(EPlayerState), reader.ReadString());
            Tilt = BSGNetworkConversionHelpers.ScaleByteToFloat(reader.ReadByte(), -5f, 5f);
            Step = reader.ReadInt32();
            AnimatorStateIndex = reader.ReadInt32();
            CharacterMovementSpeed = BSGNetworkConversionHelpers.ScaleByteToFloat(reader.ReadByte(), 0f, 1f);
            IsProne = reader.ReadBoolean();
            PoseLevel = reader.ReadSingle();
            IsSprinting = reader.ReadBoolean();
            Stamina = PhysicalUtils.Deserialize(reader);
            InputDirection = Vector2Utils.Deserialize(reader);
            LeftStance = reader.ReadBoolean();
            Blindfire = reader.ReadInt32();
            LinearSpeed = reader.ReadSingle();
            TimeSerializedBetter = reader.ReadString();

            // If has a PlayerHealth packet
            if (reader.ReadBoolean())
            {
                PlayerHealth = new PlayerHealthPacket(ProfileId);
                PlayerHealth = (PlayerHealthPacket)PlayerHealth.Deserialize(reader.ReadLengthPrefixedBytes());
            }

            //StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerStatePacket)}:{nameof(Deserialize)}:{this.SITToJson()}");
            return this;
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

        public override bool Equals(object obj)
        {
            if (obj is PlayerStatePacket other)
            {
                return other.ProfileId == ProfileId
                    && other.Position.IsEqual(Position, 1)
                    && other.Rotation.Equals(Rotation)
                    ;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected override void Process(CoopPlayerClient client)
        {
            client.ReceivePlayerStatePacket(this);
        }
    }
}
