using EFT;
using System.Collections.Generic;

namespace StayInTarkov.Coop
{
    public interface ISITGame
    {
        public List<string> ExtractedPlayers { get; }

        Dictionary<string, (float, long, string)> ExtractingPlayers { get; }

        ExitStatus MyExitStatus { get; set; }

        string MyExitLocation { get; set; }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f);

        GameStatus Status { get; }
    }
}
