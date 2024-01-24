using Aki.Custom.Airdrops.Models;
using ComponentAce.Compression.Libs.zlib;
using LiteNetLib.Utils;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.GameWorld
{
    public struct AirdropPacket() : INetSerializable
    {
        public int ConfigLength { get; set; }
        public AirdropConfigModel Config { get; set; }
        public bool AirdropAvailable { get; set; }
        public bool PlaneSpawned { get; set; }
        public bool BoxSpawned { get; set; }
        public float DistanceTraveled { get; set; }
        public float DistanceToTravel { get; set; }
        public float DistanceToDrop { get; set; }
        public float Timer { get; set; }
        public int DropHeight { get; set; }
        public int TimeToStart { get; set; }
        public Vector3 BoxPoint { get; set; }
        public Vector3 SpawnPoint { get; set; }
        public Vector3 LookPoint { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ConfigLength = reader.GetInt();
            byte[] configBytes = new byte[ConfigLength];
            reader.GetBytes(configBytes, ConfigLength);
            Config = SimpleZlib.Decompress(configBytes, null).ParseJsonTo<AirdropConfigModel>();
            AirdropAvailable = reader.GetBool();
            PlaneSpawned = reader.GetBool();
            BoxSpawned = reader.GetBool();
            DistanceTraveled = reader.GetFloat();
            DistanceToTravel = reader.GetFloat();
            DistanceToDrop = reader.GetFloat();
            Timer = reader.GetFloat();
            DropHeight = reader.GetInt();
            TimeToStart = reader.GetInt();
            BoxPoint = Vector3Utils.Deserialize(reader);
            SpawnPoint = Vector3Utils.Deserialize(reader);
            LookPoint = Vector3Utils.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            byte[] configBytes = SimpleZlib.CompressToBytes(Config.ToJson(), 9, null);
            writer.Put(configBytes.Length);
            writer.Put(configBytes);
            writer.Put(AirdropAvailable);
            writer.Put(PlaneSpawned);
            writer.Put(BoxSpawned);
            writer.Put(DistanceTraveled);
            writer.Put(DistanceToTravel);
            writer.Put(DistanceToDrop);
            writer.Put(Timer);
            writer.Put(DropHeight);
            writer.Put(TimeToStart);
            Vector3Utils.Serialize(writer, BoxPoint);
            Vector3Utils.Serialize(writer, SpawnPoint);
            Vector3Utils.Serialize(writer, LookPoint);
        }
    }
}
