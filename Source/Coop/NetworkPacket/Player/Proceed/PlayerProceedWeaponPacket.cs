using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public class PlayerProceedWeaponPacket : ItemPlayerPacket
    {
        public bool Scheduled { get; set; }

        public PlayerProceedWeaponPacket() : base("", "", "", nameof(PlayerProceedWeaponPacket))
        {

        }

        public PlayerProceedWeaponPacket(string profileId, string itemId, bool scheduled)
            : base(profileId, itemId, "", nameof(PlayerProceedWeaponPacket))
        {
            Scheduled = scheduled;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(Scheduled);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (ItemFinder.TryFindItem(ItemId, out var item))
            {
                client.Proceed((Weapon)item, (x) => { }, Scheduled);
            }
        }
    }
}
