using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Multiplayer.BTR;
using System.IO;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.BTR
{
    public sealed class BTRPacket : BasePacket
    {
        static BTRPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BTRPacket));

        }
        public BTRPacket() : base(nameof(BTRPacket))
        {
        }
        public static ManualLogSource Logger { get; }

        public string BotProfileId { get; set; }

        public Vector3? ShotDirection { get; set; }
        public Vector3? ShotPosition { get; set; }

        public bool HasShot { get { return ShotDirection.HasValue && ShotPosition.HasValue; } }

        public BTRDataPacket DataPacket { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);

            writer.Write(string.IsNullOrEmpty(BotProfileId));
            if (!string.IsNullOrEmpty(BotProfileId))
                writer.Write(BotProfileId);

            if (HasShot)
            {
                Vector3Utils.Serialize(writer, ShotDirection.Value);
                Vector3Utils.Serialize(writer, ShotPosition.Value);
            }

            writer.Write(DataPacket.BtrBotId);
            writer.Write(DataPacket.currentSpeed);
            writer.Write(DataPacket.gunsBlockRotation);
            writer.Write(DataPacket.LeftSideState);
            writer.Write(DataPacket.LeftSlot0State);
            writer.Write(DataPacket.LeftSlot1State);
            writer.Write(DataPacket.moveDirection);
            writer.Write(DataPacket.MoveSpeed);
            writer.Write(DataPacket.position);
            writer.Write(DataPacket.RightSideState);
            writer.Write(DataPacket.RightSlot0State);
            writer.Write(DataPacket.RightSlot1State);
            writer.Write(DataPacket.rotation);
            writer.Write(DataPacket.RouteState);
            writer.Write(DataPacket.State);
            writer.Write(DataPacket.timeToEndPause);
            writer.Write(DataPacket.turretRotation);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);

            if (reader.ReadBoolean())
                BotProfileId = reader.ReadString();

            if (reader.ReadBoolean())
            {
                ShotDirection = Vector3Utils.Deserialize(reader);
                ShotPosition = Vector3Utils.Deserialize(reader);
            }

            DataPacket = new()
            {
                BtrBotId = reader.ReadInt(),
                currentSpeed = reader.ReadFloat(),
                gunsBlockRotation = reader.ReadQuaternion(),
                LeftSideState = reader.ReadByte(),
                LeftSlot0State = reader.ReadByte(),
                LeftSlot1State = reader.ReadByte(),
                moveDirection = reader.ReadByte(),
                MoveSpeed = reader.ReadFloat(),
                position = Vector3Utils.Deserialize(reader),
                RightSideState = reader.ReadByte(),
                RightSlot0State = reader.ReadByte(),
                RightSlot1State = reader.ReadByte(),
                rotation = reader.ReadQuaternion(),
                RouteState = reader.ReadByte(),
                State = reader.ReadByte(),
                timeToEndPause = reader.ReadFloat(),
                turretRotation = reader.ReadQuaternion(),
            };

            return this;
        }

        public override void Process()
        {
            if (!Singleton<BTRManager>.Instantiated)
            {
                Logger.LogError($"{nameof(BTRManager)} has not been instantiated!");
                return;
            }

            Singleton<BTRManager>.Instance.BTRPacketsOnClient.Enqueue(this);
        }
    }
}
