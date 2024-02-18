using System.Collections.Generic;

namespace StayInTarkov.AkiSupport.Singleplayer.Models.ScavMode
{
    public class RaidTimeResponse
    {
        public int RaidTimeMinutes { get; set; }
        public int? NewSurviveTimeSeconds { get; set; }
        public int OriginalSurvivalTimeSeconds { get; set; }
        public List<ExitChanges> ExitChanges { get; set; }
        
    }

    public class ExitChanges
    {
        public string Name{ get; set; }
        public int? MinTime { get; set; }
        public int? MaxTime { get; set; }
        public int? Chance { get; set; }
    }
}