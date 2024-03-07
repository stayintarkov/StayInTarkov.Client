using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ToggleAimPacket : BasePlayerPacket
    {
        public ToggleAimPacket() : base("", nameof(ToggleAimPacket))
        {
        }

        public ToggleAimPacket(string profileId) : base(profileId, nameof(ToggleAimPacket))
        {
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.ToggleAim();
            }
        }
    }
}
