using BepInEx.Logging;
using EFT.UI;
using Open.Nat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public class SITIPAddresses
    {
        public SITIPAddresses() 
        { 

        }

        public class SITAddressGroup
        {
            public string IPAddressV4 { get; set; }
            public string IPAddressV6 { get; set; }

            public void ProcessIPAddressResult(string result)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // if contains : then IPv6
                    if (result.Contains(':'))
                    {
                        IPAddressV6 = result;
                    }
                    else if (result.Contains('.'))
                    {
                        IPAddressV4 = result;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }

        public SITAddressGroup ExternalAddresses { get; } = new ();   

        public SITAddressGroup InternalAddresses { get; } = new ();
       
    }

    public static class SITIPAddressManager
    {
        public static SITIPAddresses SITIPAddresses { get; } = new SITIPAddresses();
        public static ManualLogSource Logger { get; }

        public static string[] WebsitesToGetIPs = new string[] { "https://api.ipify.org/", "http://wtfismyip.com/text" }; 

        static SITIPAddressManager()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITIPAddressManager));   
        }

        public static async void GetExternalIPAddress()
        {
            // First attempt is via NAT (your router)
            if (await GetExternalIPAddressByNAT())
                return;

            // Second attempt is via web call to websites
            foreach (var address in WebsitesToGetIPs)
            {
                if (await GetExternalIPAddressByWebCall(address))
                    break;
            }

            Logger.LogDebug(SITIPAddressManager.SITIPAddresses.ExternalAddresses.IPAddressV4);

        }

        static async Task<bool> GetExternalIPAddressByNAT()
        {
            try
            {
                NatDiscoverer natDiscoverer = new NatDiscoverer();
                var device = await natDiscoverer.DiscoverDeviceAsync();
                if (device == null)
                    return false;

                var externalIp = await device.GetExternalIPAsync();
                if (externalIp == null)
                    return false;

                SITIPAddresses.ExternalAddresses.IPAddressV4 = externalIp.ToString();
                Logger.LogInfo($"External IP Discovered: {SITIPAddresses.ExternalAddresses.IPAddressV4}");
                return !string.IsNullOrEmpty(externalIp.ToString());
            }
            catch
            {

            }
            return false;
        }

        static async Task<bool> GetExternalIPAddressByWebCall(string address)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 1);
                    string result = "";
                    try
                    {
                        result = await client.GetStringAsync(address);
                        SITIPAddresses.ExternalAddresses.ProcessIPAddressResult(result);
                        return !string.IsNullOrEmpty(SITIPAddresses.ExternalAddresses.IPAddressV4);
                    }
                    catch (WebException)
                    {
                    }
                }
            }
            catch { }

            return false;
        }

    }
}
