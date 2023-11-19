using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.NetworkPacket
{
    public class DamageInfoPacket : BasePlayerPacket
    {
        [JsonProperty(PropertyName = "pid")]
        public string PlayerProfileId { get; set; }

        [JsonProperty(PropertyName = "wtid")]
        public string WeaponTemplateId { get; set; }

        [JsonProperty(PropertyName = "wid")]
        public string WeaponItemId { get; set; }

        [JsonProperty(PropertyName = "dm")]
        public string DamageInfoJson { get; set; }

        [JsonProperty(PropertyName = "pd")]
        public string PlayerDictJson { get; set; }

        [JsonProperty(PropertyName = "wd")]
        public string WeaponDictJson { get; set; }

        [JsonProperty(PropertyName = "bpt")]
        public string BodyPartType { get; set; }

        [JsonProperty(PropertyName = "bpct")]
        public string BodyPartColliderType { get; set; }

        [JsonProperty(PropertyName = "abs")]
        public float Absorbed { get; set; }

        [JsonProperty(PropertyName = "hs")]
        public float HeadSegment { get; set; }

        public DamageInfoPacket(string playerProfileId, string weaponTemplateId, string weaponItemId, DamageInfo damageInfo, string playerDictJson, string weaponDictJson, string bodyPartType, string bodyPartColliderType, float absorbed, float headSegment)
        {
            PlayerProfileId = playerProfileId;
            WeaponTemplateId = weaponTemplateId;
            WeaponItemId = weaponItemId;
            DamageInfoJson = damageInfo.ToJson();
            PlayerDictJson = playerDictJson;
            WeaponDictJson = weaponDictJson;
            BodyPartType = bodyPartType;
            BodyPartColliderType = bodyPartColliderType;
            Absorbed = absorbed;
            HeadSegment = headSegment;
        }
    }
}
