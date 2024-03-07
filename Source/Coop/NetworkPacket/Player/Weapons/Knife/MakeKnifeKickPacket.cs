using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons.Knife
{
    public sealed class MakeKnifeKickPacket : BasePlayerPacket
    {
        public MakeKnifeKickPacket() : base("", nameof(MakeKnifeKickPacket)) { }

        public MakeKnifeKickPacket(string profileId) : base(profileId, nameof(MakeKnifeKickPacket)) { }

        protected override void Process(CoopPlayerClient client)
        {
            var knifeController = client.HandsController as EFT.Player.KnifeController;
            if (knifeController != null)
            {
                knifeController.MakeKnifeKick();
            }
        }
    }
}