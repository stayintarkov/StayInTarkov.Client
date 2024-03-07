using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class CheckChamberPacket : BasePlayerPacket
    {
        public CheckChamberPacket() : base("", nameof(CheckChamberPacket)) { }

        public CheckChamberPacket(string profileId) : base(profileId, nameof(CheckChamberPacket)) { }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.CheckChamber();
            }
        }
    }
}
