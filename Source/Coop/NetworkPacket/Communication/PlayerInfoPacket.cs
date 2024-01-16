//using ComponentAce.Compression.Libs.zlib;
//using EFT;
//using LiteNetLib.Utils;

//namespace StayInTarkov.Networking.Packets
//{
//    public struct PlayerInfoPacket() : INetSerializable
//    {
//        public int ProfileLength { get; set; }
//        public Profile Profile { get; set; }

//        public void Serialize(NetDataWriter writer)
//        {
//            byte[] profileBytes = SimpleZlib.CompressToBytes(Profile.ToJson(), 9, null);
//            writer.Put(profileBytes.Length);
//            writer.Put(profileBytes);
//        }

//        public void Deserialize(NetDataReader reader)
//        {
//            ProfileLength = reader.GetInt();
//            byte[] profileBytes = new byte[ProfileLength];
//            reader.GetBytes(profileBytes, ProfileLength);
//            Profile = SimpleZlib.Decompress(profileBytes, null).ParseJsonTo<Profile>();
//        }
//    }
//}
