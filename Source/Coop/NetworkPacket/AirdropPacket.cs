using StayInTarkov.AkiSupport.Airdrops.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public sealed class AirdropPacket : BasePacket
    {
        public string AirdropParametersModelJson { get; set; }

        public AirdropPacket(AirdropParametersModel airdropParametersModel) : base("AirdropPacket")
        {
            this.AirdropParametersModelJson = airdropParametersModel.SITToJson();
        }

        public AirdropPacket() : base("AirdropPacket")
        {
        }
    }
}
