using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons.Knife
{
    public sealed class MakeKnifeAlternateKickPacket : BasePlayerPacket
    {
        public MakeKnifeAlternateKickPacket() : base("", nameof(MakeKnifeAlternateKickPacket)) { }

        public MakeKnifeAlternateKickPacket(string profileId) : base(profileId, nameof(MakeKnifeAlternateKickPacket)) { }

        protected override void Process(CoopPlayerClient client)
        {
            var knifeController = client.HandsController as EFT.Player.KnifeController;
            if (knifeController != null)
            {
                knifeController.MakeAlternativeKick();
            }
        }
    }
}
