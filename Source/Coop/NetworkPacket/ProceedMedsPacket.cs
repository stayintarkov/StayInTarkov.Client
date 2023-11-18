using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.NetworkPacket
{
    public class ProceedMedsPacket : ItemPlayerPacket
    {
        [JsonProperty(PropertyName = "bodyPart")]
        public string BodyPart { get; set; }

        [JsonProperty(PropertyName = "variant")]
        public int Variant { get; set; }

        public ProceedMedsPacket(
            string profileId, string itemId, string templateId, string bodyPart, int variant) 
            : base(profileId, itemId, templateId, "ProceedMeds")
        {
            BodyPart = bodyPart;
            Variant = variant;
        }
    }
}
