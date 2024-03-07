using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public class PickupPacket : BasePlayerPacket
    {
        public bool Pickup { get; set; }

        public PickupPacket() : base("", nameof(PickupPacket)) { }

        public PickupPacket(string profileId, bool pickup) : base(profileId, nameof(PickupPacket))
        {
            Pickup = pickup;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(Pickup);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            Pickup = reader.ReadBoolean();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.Pickup(Pickup);
            }
            var knifeController = client.HandsController as EFT.Player.KnifeController;
            if (knifeController != null)
            {
                knifeController.Pickup(Pickup);
            }
            var grenadeController = client.HandsController as EFT.Player.GrenadeController;
            if (grenadeController != null)
            {
                grenadeController.Pickup(Pickup);
            }
        }

    }
}
