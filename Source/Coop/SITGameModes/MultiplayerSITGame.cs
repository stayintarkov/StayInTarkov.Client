/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */

using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Weather;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.SITGameModes
{
    public sealed class MultiplayerSITGame : BaseLocalGame<GamePlayerOwner>, IBotGame, ISITGame
    {
        public string DisplayName => StayInTarkovPlugin.LanguageDictionary["COOP_GAME_HEADLESS"].ToString();

        public List<string> ExtractedPlayers => throw new NotImplementedException();

        public Dictionary<string, (float, long, string)> ExtractingPlayers => throw new NotImplementedException();

        public ExitStatus MyExitStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MyExitLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IGameClient GameClient => throw new NotImplementedException();

        public BotsController BotsController => throw new NotImplementedException();

        public IWeatherCurve WeatherCurve => throw new NotImplementedException();

        public int ReadyPlayers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool HostReady { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task CreateOwnPlayer(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController)
        {
            throw new NotImplementedException();
        }

        public Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryControllerClass inventoryController, Callback runCallback)
        {
            throw new NotImplementedException();
        }
    }
}
