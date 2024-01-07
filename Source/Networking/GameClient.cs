using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Coop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public static class GameClient
    {
        static ManualLogSource Logger { get; set; }

        static GameClient()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("GameClient");
        }

        public static void SendDataToServer(byte[] data)
        {
            if (!Singleton<ISITGame>.Instantiated)
            {
                Logger.LogError($"{nameof(ISITGame)}:{nameof(SendDataToServer)} has not been Instantiated");
                return;
            }



        }
    }
}
