using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Multiplayer.BTR;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.SITGameModes
{
    public static class SITGameModeHelpers
    {
        public static ManualLogSource Logger { get; }

        static SITGameModeHelpers()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITGameModeHelpers));
        }
        public static async Task UpdateRaidStatusAsync(SITMPRaidStatus status)
        {
            JObject jobj = new();
            jobj.Add("status", status.ToString());
            jobj.Add("serverId", SITGameComponent.GetServerId());
            await AkiBackendCommunication.Instance.PostJsonAsync("/coop/server/update", jobj.ToString());
        }

        public static void InitBTR()
        {
            try
            {
                var btrSettings = Singleton<BackendConfigSettingsClass>.Instance.BTRSettings;
                var gameWorld = Singleton<GameWorld>.Instance;

                // Only run on maps that have the BTR enabled
                string location = gameWorld.MainPlayer.Location;
                if (!btrSettings.LocationsWithBTR.Contains(location))
                {
                    return;
                }

                gameWorld.gameObject.AddComponent<BTRManager>();
            }
            catch (System.Exception ex)
            {
                ConsoleScreen.LogError($"{nameof(InitBTR)} Error!");
                Logger.LogError(ex);
                throw;
            }
        }
    }
}
