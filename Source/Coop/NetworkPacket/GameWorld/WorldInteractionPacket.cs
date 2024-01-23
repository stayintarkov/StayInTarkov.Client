using EFT;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityStandardAssets.Water;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.GameWorld
{
    public class WorldInteractionPacket : BasePacket, ISITPacket, INetSerializable
    {
        public WorldInteractionPacket() : base("WorldInteractionPacket")
        {
        }

        public string InteractiveId { get; set; }
        public EInteractionType InteractionType { get; set; }
        public bool IsStart { get; set; }
        public bool HasKey { get; set; }
        public string KeyItemId { get; set; }
        public string KeyItemTemplateId { get; set; }
        public GridItemAddressDescriptor GridItemAddressDescriptor { get; set; }
        public bool KeySuccess { get; set; }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));

            InteractiveId = reader.ReadString();
            InteractionType = (EInteractionType)reader.ReadInt32();
            IsStart = reader.ReadBoolean();
            HasKey = reader.ReadBoolean();
            if (HasKey)
            {
                KeyItemId = reader.ReadString();
                KeyItemTemplateId = reader.ReadString();
                GridItemAddressDescriptor = AddressUtils.DeserializeGridItemAddressDescriptor(reader);
                KeySuccess = reader.ReadBoolean();
            }
            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);    
            writer.Write(InteractiveId);
            writer.Write((int)InteractionType);
            writer.Write(IsStart);
            writer.Write(HasKey);
            if (HasKey)
            {
                writer.Write(KeyItemId);
                writer.Write(KeyItemTemplateId);
                AddressUtils.SerializeGridItemAddressDescriptor(writer, GridItemAddressDescriptor);
                writer.Write(KeySuccess);
            }
            return ms.ToArray();
        }

        internal WorldInteractionPacket Deserialize(NetDataReader reader)
        {
            InteractiveId = reader.GetString();
            InteractionType = (EInteractionType)reader.GetInt();
            IsStart = reader.GetBool();
            HasKey = reader.GetBool();
            if (HasKey)
            {
                KeyItemId = reader.GetString();
                KeyItemTemplateId = reader.GetString();
                GridItemAddressDescriptor = AddressUtils.DeserializeGridItemAddressDescriptor(reader);
                KeySuccess = reader.GetBool();
            }
            return this;
        }

        internal void Serialize(NetDataWriter writer, WorldInteractionPacket worldInteractionPacket)
        {
            throw new NotImplementedException();
        }
    }
}
