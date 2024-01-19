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

            var stunUdpClient = new UdpClient(AddressFamily.InterNetwork);
            stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));

            try
            {
                IPAddress stunIp = Array.Find(Dns.GetHostEntry("stun.l.google.com").AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                int stunPort = 19302;

                var stunEndPoint = new IPEndPoint(stunIp, stunPort);

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
