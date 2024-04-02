using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.SITGameModes.Headless
{
    public static class HeadlessHelpers
    {
        static HeadlessHelpers()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(HeadlessHelpers));
        }

        public static ManualLogSource Logger { get; }

        public static void StartGame(string locationName)
        {
            if (Singleton<ClientApplication<ISession>>.Instantiated)
            {
                Logger.LogDebug($"Force start of game in Headless!");

                var tarkovApp = Singleton<ClientApplication<ISession>>.Instance as TarkovApplication;
                SITMatchmaking.MatchingType = EMatchmakerType.HeadlessHost;
                tarkovApp.InternalStartGame(locationName, true, true);
                var raidSettings = (RaidSettings)ReflectionHelpers.GetFieldFromTypeByFieldType(tarkovApp.GetType(), typeof(RaidSettings)).GetValue(tarkovApp);
                SITMatchmaking.CreateMatch(
                   tarkovApp.Session.Profile.ProfileId
                   , raidSettings
                   , ""
                   , ESITProtocol.PeerToPeerUdp
                   , null
                   , 6972
                   , EMatchmakerType.HeadlessHost);
            }
        }
    }
}
