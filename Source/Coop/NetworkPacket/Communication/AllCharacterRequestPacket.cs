using LiteNetLib.Utils;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Communication
{
    public struct AllCharacterRequestPacket : INetSerializable
    {
        public bool IsRequest { get; set; } = true;
        public string ProfileId { get; set; }
        public int CharactersAmount { get; set; } = 0;
        public string[] Characters { get; set; }
        public PlayerInfoPacket PlayerInfo { get; set; }
        public bool IsAlive { get; set; } = true;
        public Vector3 Position { get; set; }

        public AllCharacterRequestPacket(string profileId)
        {
            ProfileId = profileId;
        }

        public void Deserialize(NetDataReader reader)
        {
            IsRequest = reader.GetBool();
            ProfileId = reader.GetString();
            CharactersAmount = reader.GetInt();
            if (CharactersAmount > 0)
            {
                Characters = new string[CharactersAmount];
                for (int i = 0; i < CharactersAmount; i++)
                {
                    Characters[i] = reader.GetString();
                }
            }
            if (!IsRequest)
            {
                PlayerInfo = PlayerInfoPacket.Deserialize(reader);
            }
            IsAlive = reader.GetBool();
            Position = Vector3Utils.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsRequest);
            writer.Put(ProfileId);
            writer.Put(CharactersAmount);
            if (CharactersAmount > 0)
            {
                for (int i = 0; i < CharactersAmount; i++)
                {
                    writer.Put(Characters[i]);
                }
            }
            if (!IsRequest)
            {
                PlayerInfoPacket.Serialize(writer, PlayerInfo);
            }
            writer.Put(IsAlive);
            Vector3Utils.Serialize(writer, Position);
        }
    }
}
