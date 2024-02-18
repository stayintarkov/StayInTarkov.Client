using EFT;

namespace StayInTarkov.AkiSupport.Singleplayer.Models.ScavMode
{
    public class RaidTimeRequest
    {
        public RaidTimeRequest(ESideType side, string location)
        {
            Side = side;
            Location = location;
        }

        public ESideType Side { get; set; }
        public string Location { get; set; }
    }
}
