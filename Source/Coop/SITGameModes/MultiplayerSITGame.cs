/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */

using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Interactive;
using EFT.Weather;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.SITGameModes
{
    public sealed class MultiplayerSITGame : BaseLocalGame<EftGamePlayerOwner>, IBotGame, ISITGame
    {
        public string DisplayName { get; } = "Coop Game";

        public List<string> ExtractedPlayers => throw new NotImplementedException();

        public Dictionary<string, (float, long, string)> ExtractingPlayers => throw new NotImplementedException();

        public List<ExfiltrationPoint> EnabledCountdownExfils => throw new NotImplementedException();

        public float PastTime => throw new NotImplementedException();

        public ExitStatus MyExitStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MyExitLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IGameClient GameClient => throw new NotImplementedException();

        public BotsController BotsController => throw new NotImplementedException();

        public IWeatherCurve WeatherCurve => throw new NotImplementedException();

        public int ReadyPlayers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool HostReady { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task Run(BotControllerSettings botsSettings, string backendUrl, InventoryControllerClass inventoryController, Callback runCallback)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator vmethod_1()
        {
            throw new NotImplementedException();
        }
    }
}
