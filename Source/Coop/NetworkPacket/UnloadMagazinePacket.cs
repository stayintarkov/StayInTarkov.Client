using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.NetworkPacket
{
    public class UnloadMagazinePacket : BasePlayerPacket
    {
        public string MagazineId { get; set; }
        public string MagazineTemplateId { get; set; }

        public UnloadMagazinePacket(
            string profileId
            , string magazineId
            , string magazineTemplateId
            )
            : base(profileId, "PlayerInventoryController_UnloadMagazine")
        {
            this.MagazineId = magazineId;
            this.MagazineTemplateId = magazineTemplateId;
        }
    }
}
