using LiteNetLib.Utils;
using static StayInTarkov.Networking.SITSerialization;

// Look at Packet3 in DnSpy

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client). 
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License.
*/

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public struct HealthPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool HasDamageInfo { get; set; }
        public ApplyDamageInfoPacket ApplyDamageInfo { get; set; }
        public bool HasBodyPartRestoreInfo { get; set; }
        public RestoreBodyPartPacket RestoreBodyPartPacket { get; set; }
        public bool HasBodyPartDestroyInfo { get; set; }
        public DestroyBodyPartPacket DestroyBodyPartPacket { get; set; }
        public bool HasChangeHealthPacket { get; set; }
        public ChangeHealthPacket ChangeHealthPacket { get; set; }
        public bool HasEnergyChange { get; set; }
        public float EnergyChangeValue { get; set; }
        public bool HasHydrationChange { get; set; }
        public float HydrationChangeValue { get; set; }
        public bool HasAddEffect { get; set; }
        public AddEffectPacket AddEffectPacket { get; set; }
        public bool HasRemoveEffect { get; set; }
        public RemoveEffectPacket RemoveEffectPacket { get; set; }
        public bool HasObservedDeathPacket { get; set; }
        public ObservedDeathPacket ObservedDeathPacket { get; set; }

        public HealthPacket(string profileId)
        {
            ProfileId = profileId;
            HasDamageInfo = false;
            HasBodyPartRestoreInfo = false;
            HasBodyPartDestroyInfo = false;
            HasChangeHealthPacket = false;
            HasEnergyChange = false;
            HasHydrationChange = false;
            HasAddEffect = false;
            HasRemoveEffect = false;
            HasObservedDeathPacket = false;
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
            HasBodyPartDestroyInfo = reader.GetBool();
            if (HasBodyPartDestroyInfo)
                DestroyBodyPartPacket = DestroyBodyPartPacket.Deserialize(reader);
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
                AddEffectPacket = AddEffectPacket.Deserialize(reader);
            HasRemoveEffect = reader.GetBool();
            if (HasRemoveEffect)
                RemoveEffectPacket = RemoveEffectPacket.Deserialize(reader);
            HasObservedDeathPacket = reader.GetBool();
            if (HasObservedDeathPacket)
                ObservedDeathPacket = ObservedDeathPacket.Deserialize(reader);
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
            writer.Put(HasBodyPartDestroyInfo);
            if (HasBodyPartDestroyInfo)
                DestroyBodyPartPacket.Serialize(writer, DestroyBodyPartPacket);
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
                AddEffectPacket.Serialize(writer, AddEffectPacket);
            writer.Put(HasRemoveEffect);
            if (HasRemoveEffect)
                RemoveEffectPacket.Serialize(writer, RemoveEffectPacket);
            writer.Put(HasObservedDeathPacket);
            if (HasObservedDeathPacket)
                ObservedDeathPacket.Serialize(writer, ObservedDeathPacket);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
