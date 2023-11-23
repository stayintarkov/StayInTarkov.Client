using EFT;
using System.Collections.Generic;

namespace StayInTarkov.Coop
{
    internal interface ISITGame
    {
        public List<string> ExtractedPlayers { get; }

        Dictionary<string, (float, long, string)> ExtractingPlayers { get; }

        ExitStatus MyExitStatus { get; set; }

        string MyExitLocation { get; set; }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f);
    }
}
