/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */


using EFT;
using StayInTarkov.Networking;
using System.Collections.Generic;

namespace StayInTarkov.Coop.SITGameModes
{
    public interface ISITGame
    {
        public List<string> ExtractedPlayers { get; }

        Dictionary<string, (float, long, string)> ExtractingPlayers { get; }

        ExitStatus MyExitStatus { get; set; }

        string MyExitLocation { get; set; }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f);

        public IGameClient GameClient { get; }

    }
}
