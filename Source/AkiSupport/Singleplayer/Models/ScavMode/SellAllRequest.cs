using EFT.InventoryLogic.BackendInventoryInteraction;
using Newtonsoft.Json;


namespace StayInTarkov.AkiSupport.Singleplayer.Models.ScavMode
{
    public class SellAllRequest
    {
        [JsonProperty("Action")]
        public string Action;

        [JsonProperty("totalValue")]
        public int TotalValue;

        [JsonProperty("fromOwner")]
        public OwnerInfo FromOwner;

        [JsonProperty("toOwner")]
        public OwnerInfo ToOwner;

    }
}

