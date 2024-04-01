/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */


using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Interactive;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.SITGameModes
{
    public interface ISITGame
    {
        public string DisplayName { get; }

        public List<string> ExtractedPlayers { get; }

        public int ReadyPlayers { get; set; }
        public bool HostReady { get; set; }

        Dictionary<string, (float, long, string)> ExtractingPlayers { get; }

        /// <summary>
        /// ExfiltrationPoints that have started counting down. They will extract all Entered players upon reaching 0
        /// They will be disabled if not reusable
        /// </summary>
        public List<ExfiltrationPoint> EnabledCountdownExfils { get; }

        public float PastTime { get; }

        ExitStatus MyExitStatus { get; set; }

        string MyExitLocation { get; set; }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f);

        public IGameClient GameClient { get; }

        public Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryControllerClass inventoryController, Callback runCallback);


        //Task WaitForPlayersToSpawn();
        //Task WaitForPlayersToBeReady();

    }
}
