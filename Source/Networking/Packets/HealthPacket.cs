using LiteNetLib.Utils;
using System;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Networking.Packets
{
    public struct HealthPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool HasDamageInfo { get; set; }
        public SITSerialization.ApplyDamageInfoPacket ApplyDamageInfo { get; set; }

        public HealthPacket(string profileId)
        {
            ProfileId = profileId;
        }

        public void Deserialize(NetDataReader reader)
        {
            HasDamageInfo = reader.GetBool();
            if (HasDamageInfo)
                ApplyDamageInfo = SITSerialization.ApplyDamageInfoPacket.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(HasDamageInfo);
            if (HasDamageInfo)
                SITSerialization.ApplyDamageInfoPacket.Serialize(writer, ApplyDamageInfo);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
