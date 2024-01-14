using StayInTarkov.Configuration;
using STUN.Attributes;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace StayInTarkov.Networking
{
    public static class STUNHelper
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("STUN Helper");

        public static bool Query(int localPort, out STUNQueryResult queryResult)
        {
            queryResult = new STUNQueryResult();

            var stunUdpClient = new UdpClient();
            stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));

            // Google's STUN server should be pretty safe to use in the long term.
            if (!STUNUtils.TryParseHostAndPort("stun.l.google.com:19302", out IPEndPoint stunEndPoint))
                throw new Exception("Failed to resolve STUN server address!");

            try
            {
                queryResult = STUNClient.Query(stunUdpClient.Client, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);
                //var queryResult = STUNClient.Query(stunEndPoint, STUNQueryType.ExactNAT, true, NATTypeDetectionRFC.Rfc3489);
            }
            catch (STUNQueryErrorException ex)
            {
                Logger.LogError(ex.Error);
                return false;
            }

            stunUdpClient.Client.Close();

            return true;
        }
    }
}
