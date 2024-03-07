using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class ExamineWeaponPacket : BasePlayerPacket
    {
        public ExamineWeaponPacket() : base("", nameof(ExamineWeaponPacket)) { }

        public ExamineWeaponPacket(string profileId) : base(profileId, nameof(ExamineWeaponPacket)) { }

        protected override void Process(CoopPlayerClient client)
        {
            var firearmController = client.HandsController as EFT.Player.FirearmController;
            if (firearmController != null)
            {
                firearmController.ExamineWeapon();
            }
            var knifeController = client.HandsController as EFT.Player.KnifeController;
            if (knifeController != null)
            {
                knifeController.ExamineWeapon();
            }
            var grenadeController = client.HandsController as EFT.Player.GrenadeController;
            if (grenadeController != null)
            {
                grenadeController.ExamineWeapon();
            }
        }
    }
}
