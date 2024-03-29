/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */


using Comfort.Common;
using EFT;
using EFT.Bots;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.SITGameModes
{
    public interface ISITGame
    {
        public string DisplayName { get; }

        public List<string> ExtractedPlayers { get; }

        public int ReadyPlayers { get; set; }
        public bool HostReady { get; set; }

        Dictionary<string, (float, long, string)> ExtractingPlayers { get; }

        ExitStatus MyExitStatus { get; set; }

        string MyExitLocation { get; set; }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f);

        public IGameClient GameClient { get; }

        public Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryControllerClass inventoryController, Callback runCallback);

        public Task CreateOwnPlayer(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController);


        //Task WaitForPlayersToSpawn();
        //Task WaitForPlayersToBeReady();

    }
}
