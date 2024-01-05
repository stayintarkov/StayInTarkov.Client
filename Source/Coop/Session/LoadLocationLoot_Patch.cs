using Aki.Custom.Airdrops.Models;
using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking.Match;
using UnityEngine.Profiling;

namespace StayInTarkov.Coop.Session
{
    public class LocationDataRequest
    {
        [JsonProperty("data")]
        public LocationSettings.Location Data { get; set; }
    }

    public class LoadLocationLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(typeof(Session1), "LoadLocationLoot");

            Logger.LogDebug($"{GetType().Name} Method: {method?.Name}");

            return method;
        }

        [PatchPrefix]
        private static bool PatchPrefix(string locationId, int variantId, ref Task<LocationSettings.Location> __result)
        {
            Logger.LogDebug("LoadLocationLoot PatchPrefix");

            if (MatchmakerAcceptPatches.MatchingType == EMatchmakerType.Single)
            {
                return true;
            }

            string serverId = MatchmakerAcceptPatches.GetGroupId();

            var objectToSend = new Dictionary<string, object>
            {
                { "locationId", locationId }
                , { "variantId", variantId }
                , { "serverId", serverId }
            };

            __result = Task.Run(() =>
            {
                var result = AkiBackendCommunication.Instance.PostJson($"/coop/location/getLoot", JsonConvert.SerializeObject(objectToSend));
                result.TrySITParseJson(out LocationDataRequest locationDataRequest);
                if (locationDataRequest != null)
                    return locationDataRequest.Data;

                return null;
            });

            // Protection. If the __result is null, then run the original method
            return __result == null;
        }
    }
}
