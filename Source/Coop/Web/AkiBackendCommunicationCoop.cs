using StayInTarkov.Networking;
using System;
using System.Collections.Generic;

namespace StayInTarkov.Coop.Web
{
    public class AkiBackendCommunicationCoop : AkiBackendCommunication
    {
        static Random Randomizer { get; }

        /// <summary>
        /// Static Constructor is run when the Assembly is loaded
        /// </summary>
        static AkiBackendCommunicationCoop()
        {
            Randomizer = new Random();
        }

        /// <summary>
        /// Constructor of this instance
        /// </summary>
        AkiBackendCommunicationCoop() : base(null)
        {
        }

        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
            )
        {
            PostLocalPlayerData(player, data, out _, out _);
        }

        /// <summary>
        /// Posts the data to the Udp Socket and returns the changed Dictionary for any extra use
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="useWebSocket">Use the Web Socket (faster than HTTP)</param>
        /// <returns></returns>
        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
            , out string returnedData
            , out Dictionary<string, object> generatedData)
        {
            returnedData = string.Empty;

            if (!data.ContainsKey("t"))
            {
                data.Add("t", DateTime.Now.Ticks.ToString("G"));
            }
            if (!data.ContainsKey("serverId"))
            {
                data.Add("serverId", CoopGameComponent.GetServerId());
            }
            if (!data.ContainsKey("profileId"))
            {
                data.Add("profileId", player.ProfileId);
            }
            AkiBackendCommunication.Instance.SendDataToPool("", data);
            generatedData = data;
        }
    }
}
