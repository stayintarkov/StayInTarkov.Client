using BepInEx.Logging;
using Comfort.Common;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.SITGameModes;
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

        public static void SendData(byte[] data)
        {
            if (!Singleton<ISITGame>.Instantiated)
            {
                Logger.LogError($"{nameof(GameClient)}:{nameof(SendData)} {nameof(ISITGame)} has not been Instantiated");
                return;
            }


            if (Singleton<ISITGame>.Instance.GameClient == null)
            {
                Logger.LogError($"{nameof(ISITGame)}:{nameof(IGameClient)} has not been Instantiated");
                return;
            }

//#if DEBUG
//            Logger.LogInfo("SendData(byte[] data)");
//            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
//            Logger.LogInfo($"{t.ToString()}");
//#endif

            Singleton<ISITGame>.Instance.GameClient.SendData(data);
        }

        public static void SendData<T>(ref T packet) where T : BasePacket
        {
            if (!Singleton<ISITGame>.Instantiated)
            {
                Logger.LogError($"{nameof(ISITGame)}:{nameof(SendData)} has not been Instantiated");
                return;
            }

            if (Singleton<ISITGame>.Instance.GameClient == null)
            {
                Logger.LogError($"{nameof(ISITGame)}:{nameof(IGameClient)} has not been Instantiated");
                return;
            }

            Logger.LogInfo($"SendData({packet.GetType()})");

            Singleton<ISITGame>.Instance.GameClient.SendData(ref packet);
        }

        public static void SendData(string data)
        {
            SendData(Encoding.UTF8.GetBytes(data));
        }
    }
}
