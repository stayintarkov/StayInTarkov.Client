using LiteNetLib.Utils;
using System;
using System.Configuration;
using static StayInTarkov.Networking.SITSerialization;

// Look at Packet3 in DnSpy

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Networking.Packets
{
    public struct HealthPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool HasDamageInfo { get; set; }
        public ApplyDamageInfoPacket ApplyDamageInfo { get; set; }
        public bool HasBodyPartRestoreInfo { get; set; }
        public RestoreBodyPartPacket RestoreBodyPartPacket { get; set; }
        public bool HasChangeHealthPacket { get; set; }
        public ChangeHealthPacket ChangeHealthPacket { get; set; }
        public bool HasEnergyChange { get; set; }
        public float EnergyChangeValue { get; set; }
        public bool HasHydrationChange { get; set; }
        public float HydrationChangeValue { get; set; }
        public bool HasAddEffect { get; set; }
        public AddEffectPacket AddEffectPacket { get; set; }

        public HealthPacket(string profileId)
        {
            ProfileId = profileId;
            HasDamageInfo = false;
            HasBodyPartRestoreInfo = false;
            HasChangeHealthPacket = false;
            HasEnergyChange = false;
            HasHydrationChange = false;
            HasAddEffect = false;
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            HasDamageInfo = reader.GetBool();
            if (HasDamageInfo)
                ApplyDamageInfo = ApplyDamageInfoPacket.Deserialize(reader);
            HasBodyPartRestoreInfo = reader.GetBool();
            if (HasBodyPartRestoreInfo)
                RestoreBodyPartPacket = RestoreBodyPartPacket.Deserialize(reader);
            HasChangeHealthPacket = reader.GetBool();
            if (HasChangeHealthPacket)
                ChangeHealthPacket = ChangeHealthPacket.Deserialize(reader);
            HasEnergyChange = reader.GetBool();
            if (HasEnergyChange)
                EnergyChangeValue = reader.GetFloat();
            HasHydrationChange = reader.GetBool();
            if (HasHydrationChange)
                HydrationChangeValue = reader.GetFloat();
            HasAddEffect = reader.GetBool();
            if (HasAddEffect)
                AddEffectPacket = SITSerialization.AddEffectPacket.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(HasDamageInfo);
            if (HasDamageInfo)
                ApplyDamageInfoPacket.Serialize(writer, ApplyDamageInfo);
            writer.Put(HasBodyPartRestoreInfo);
            if (HasBodyPartRestoreInfo)
                RestoreBodyPartPacket.Serialize(writer, RestoreBodyPartPacket);
            writer.Put(HasChangeHealthPacket);
            if (HasChangeHealthPacket)
                ChangeHealthPacket.Serialize(writer, ChangeHealthPacket);
            writer.Put(HasEnergyChange);
            if (HasEnergyChange)
                writer.Put(HydrationChangeValue);
            writer.Put(HasHydrationChange);
            if (HasHydrationChange)
                writer.Put(HydrationChangeValue);
            writer.Put(HasAddEffect);
            if (HasAddEffect)
                SITSerialization.AddEffectPacket.Serialize(writer, AddEffectPacket);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
