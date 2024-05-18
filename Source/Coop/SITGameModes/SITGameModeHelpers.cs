using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components.CoopGameComponents;
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
        public static async Task UpdateRaidStatusAsync(SITMPRaidStatus status)
        {
            JObject jobj = new JObject();
            jobj.Add("status", status.ToString());
            jobj.Add("serverId", SITGameComponent.GetServerId());
            await AkiBackendCommunication.Instance.PostJsonAsync("/coop/server/update", jobj.ToString());
        }
    }
}
