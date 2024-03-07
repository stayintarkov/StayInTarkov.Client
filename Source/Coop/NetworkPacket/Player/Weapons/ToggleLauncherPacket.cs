using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ToggleLauncherPacket : BasePlayerPacket
    {
        public ToggleLauncherPacket() : base("", nameof(ToggleLauncherPacket))
        {
        }

        public ToggleLauncherPacket(string profileId) : base(profileId, nameof(ToggleLauncherPacket))
        {
        }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.ToggleLauncher();
            }
        }
    }
}
