using Aki.Custom.Airdrops.Models;
using ComponentAce.Compression.Libs.zlib;
using LiteNetLib.Utils;

namespace StayInTarkov.Coop.NetworkPacket.GameWorld
{
    public struct AirdropLootPacket(bool isRequest = false) : INetSerializable
    {
        public bool IsRequest { get; set; } = isRequest;
        public int LootLength { get; set; }
        public AirdropLootResultModel Loot { get; set; }
        public int ConfigLength { get; set; }
        public AirdropConfigModel Config { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            IsRequest = reader.GetBool();
            LootLength = reader.GetInt();
            byte[] modelBytes = new byte[LootLength];
            reader.GetBytes(modelBytes, LootLength);
            Loot = SimpleZlib.Decompress(modelBytes, null).ParseJsonTo<AirdropLootResultModel>();
            ConfigLength = reader.GetInt();
            byte[] configBytes = new byte[ConfigLength];
            reader.GetBytes(configBytes, ConfigLength);
            Config = SimpleZlib.Decompress(configBytes, null).ParseJsonTo<AirdropConfigModel>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsRequest);
            byte[] modelBytes = SimpleZlib.CompressToBytes(Loot.ToJson(), 9, null);
            writer.Put(modelBytes.Length);
            writer.Put(modelBytes);
            byte[] configBytes = SimpleZlib.CompressToBytes(Config.ToJson(), 9, null);
            writer.Put(configBytes.Length);
            writer.Put(configBytes);
        }
    }
}
