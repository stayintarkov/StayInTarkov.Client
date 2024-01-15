using System.IO;
using UnityStandardAssets.Water;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class ItemPlayerPacket : BasePlayerPacket
    {
        public string ItemId { get; set; }

        public string TemplateId { get; set; }

        public byte[] OperationBytes { get; set; }
        public uint CallbackId { get; set; }
        public string InventoryId { get; set; }

        public ItemPlayerPacket(string profileId, string itemId, string templateId, string method)
            : base(new string(profileId.ToCharArray()), method)
        {
            ItemId = itemId;
            TemplateId = templateId;
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            File.WriteAllBytes($"DEBUG_{nameof(ItemPlayerPacket)}_{nameof(Deserialize)}.bin", bytes);

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);

            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},ReadHeader [{reader.BaseStream.Position}]");

            ProfileId = reader.ReadString();

            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read {nameof(ProfileId)} {ProfileId} [{reader.BaseStream.Position}]");

            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return this;

            ItemId = reader.ReadString();

            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read {nameof(ItemId)} {ItemId} [{reader.BaseStream.Position}]");

            TemplateId = reader.ReadString();

            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read {nameof(TemplateId)} {TemplateId} [{reader.BaseStream.Position}]");

            var hasBytes = reader.ReadBoolean();
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return this;

            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read hasBytes [{reader.BaseStream.Position}]");

            if (hasBytes)
            {
                OperationBytes = reader.ReadLengthPrefixedBytes();

                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read OperationBytes [{reader.BaseStream.Position}]");

                CallbackId = reader.ReadUInt32();

                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read CallbackId");

                InventoryId = reader.ReadString();

                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Deserialize)},Read InventoryId");

            }

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(TemplateId);

            var hasBytes = OperationBytes != null && OperationBytes.Length > 0;
            writer.Write(hasBytes);
            if (hasBytes)
            {
                writer.WriteLengthPrefixedBytes(OperationBytes);

                StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ItemPlayerPacket)},{nameof(Serialize)},Written {nameof(OperationBytes)} at Length {OperationBytes.Length}");

                writer.Write(CallbackId);
                writer.Write(InventoryId);
            }

            var bytes = ms.ToArray();

#if DEBUG
            File.WriteAllBytes($"DEBUG_{nameof(ItemPlayerPacket)}_{nameof(Serialize)}.bin", bytes);
#endif

            return bytes;
        }
    }
}