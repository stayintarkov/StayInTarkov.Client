using SIT.Core.Coop;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;

namespace SIT.Coop.Core.Web
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
            , bool useWebSocket = false
            )
        {
            PostLocalPlayerData(player, data, useWebSocket, out _, out _);
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
            , bool useWebSocket
            , out string returnedData
            , out Dictionary<string, object> generatedData)
        {
            returnedData = string.Empty;

            if (!data.ContainsKey("t"))
            {
                data.Add("t", DateTime.Now.Ticks.ToString("G"));
            }
            if (!data.ContainsKey("tkn"))
            {
                data.Add("tkn", Randomizer.NextDouble());
            }
            if (!data.ContainsKey("serverId"))
            {
                data.Add("serverId", CoopGameComponent.GetServerId());
            }
            if (!data.ContainsKey("profileId"))
            {
                data.Add("profileId", player.ProfileId); // PatchConstants.GetPlayerProfileAccountId(profile));
            }
            AkiBackendCommunication.Instance.SendDataToPool("", data);
            generatedData = data;
        }
    }
}
