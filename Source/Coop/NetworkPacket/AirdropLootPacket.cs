using Aki.Custom.Airdrops.Models;
using StayInTarkov.AkiSupport.Airdrops.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    public sealed class AirdropLootPacket : BasePacket
    {
        public string AirdropLootResultModelJson { get; set; }
        public string AirdropConfigModelJson { get; set; }

        public AirdropLootPacket(AirdropLootResultModel airdropLootResultModel, AirdropConfigModel airdropConfigModel) : base("AirdropLootPacket")
        {
            this.AirdropLootResultModelJson = airdropLootResultModel.SITToJson();
            this.AirdropConfigModelJson = airdropConfigModel.SITToJson();
        }

        public AirdropLootPacket() : base("AirdropLootPacket")
        {
        }
    }
}
