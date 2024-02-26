using EFT.InventoryLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public class FireModePacket : BasePlayerPacket
    {
        [JsonProperty("f")]
        public byte FireMode { get; set; }

        public FireModePacket() : this("", Weapon.EFireMode.single)
        {

        }

        public FireModePacket(string profileId, Weapon.EFireMode fireMode)
            : base(profileId, "ChangeFireMode")
        {
            FireMode = (byte)fireMode;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(FireMode);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            FireMode = reader.ReadByte();
            return this;
        }
    }
}
