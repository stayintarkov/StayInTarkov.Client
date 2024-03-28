using StayInTarkov.Coop.Players;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public sealed class PlayerPostProceedDataSyncPacket : BasePlayerPacket
    {
        public PlayerPostProceedDataSyncPacket() : base("", nameof(PlayerPostProceedDataSyncPacket)) { }

        public PlayerPostProceedDataSyncPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerPostProceedDataSyncPacket))
        {

        }

        public PlayerPostProceedDataSyncPacket(string profileId, string itemId, float newValue, int stackItemCount) : this(new string(profileId.ToCharArray()))
        {
            ProfileId = profileId;
            ItemId = itemId;
            NewValue = newValue;
            StackObjectsCount = stackItemCount;
        }

        public string ItemId { get; set; }

        public float NewValue { get; set; }

        public int StackObjectsCount { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(NewValue);
            writer.Write((byte)StackObjectsCount);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            ItemId = reader.ReadString();
            NewValue = reader.ReadSingle();
            StackObjectsCount = reader.ReadByte();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (Method != nameof(PlayerPostProceedDataSyncPacket))
                return;

            //StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            client.ReplicatedPostProceedData.Enqueue(this);
        }

        public override string ToString()
        {
            return this.ToJson();
        }

    }
}
