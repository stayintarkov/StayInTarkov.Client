using Newtonsoft.Json;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Raid;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine.UIElements.UIR.Implementation;
using UnityEngine.VR;
using UnityEngineInternal.XR.WSA;

namespace StayInTarkov.Coop.Session
{
    public class LocationDataRequest
    {
        [JsonProperty("data")]
        public LocationSettingsClass.Location Data { get; set; }
    }

    public class LoadLocationLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(typeof(TradingBackend), "LoadLocationLoot");

            Logger.LogDebug($"{GetType().Name} Method: {method?.Name}");

            return method;
        }

        [PatchPrefix]
        private static bool PatchPrefix(string locationId, int variantId, ref Task<LocationSettingsClass.Location> __result)
        {
            Logger.LogDebug("LoadLocationLoot PatchPrefix");

            if (SITMatchmaking.MatchingType == EMatchmakerType.Single)
            {
                return true;
            }

            string serverId = SITMatchmaking.GetGroupId();

            var objectToSend = new Dictionary<string, object>
            {
                { "locationId", locationId }
                , { "variantId", variantId }
                , { "serverId", serverId }
            };

            var rsp = AkiBackendCommunication.Instance.PostJsonBLOCKING($"/coop/location/getLoot", JsonConvert.SerializeObject(objectToSend));
            if (rsp == null)
            {
                return true;
            }

            rsp.TrySITParseJson(out LocationDataRequest locationDataRequest);
            if (locationDataRequest == null)
            {
                return true;
            }

            __result = Task.FromResult(locationDataRequest.Data);
            return false;
        }
    }
}
