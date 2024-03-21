using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices
{
    public class TraderServiceModel
    {
        [JsonProperty("serviceType")]
        public ETraderServiceType ServiceType { get; set; }

        [JsonProperty("itemsToPay")]
        public Dictionary<MongoID, int> ItemsToPay { get; set; }

        [JsonProperty("subServices")]
        public Dictionary<string, int> SubServices { get; set; }

        [JsonProperty("itemsToReceive")]
        public MongoID[] ItemsToReceive { get; set; }

        [JsonProperty("requirements")]
        public TraderServiceRequirementsModel Requirements { get; set; }
    }

    public class TraderServiceRequirementsModel
    {
        [JsonProperty("completedQuests")]
        public string[] CompletedQuests { get; set; }

        [JsonProperty("standings")]
        public Dictionary<string, float> Standings { get; set; }

        [JsonProperty("side")]
        public ESideType Side { get; set; }
    }
}