using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop
{
    internal class CoopFirearmController : FirearmController
    {
        public override void AimingChanged(bool newValue)
        {
            base.AimingChanged(newValue);
        }
    }
}
