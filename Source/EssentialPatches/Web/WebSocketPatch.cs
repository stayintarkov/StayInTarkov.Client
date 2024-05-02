using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov
{
    public class WebSocketPatch : ModulePatch
    {
        public static bool IsHttps = true;

        protected override MethodBase GetTargetMethod()
        {
            var targetInterface = StayInTarkovHelperConstants.EftTypes.SingleCustom(x => x == typeof(IConnectionHandler) && x.IsInterface);
            var typeThatMatches = StayInTarkovHelperConstants.EftTypes.SingleCustom(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);

            return typeThatMatches.GetMethods(BindingFlags.Public | BindingFlags.Instance).SingleCustom(x => x.ReturnType == typeof(Uri));
        }

        // This is a pass through postfix and behaves a little differently than usual
        // https://harmony.pardeike.net/articles/patching-postfix.html#pass-through-postfixes
        [PatchPostfix]
        private static Uri PatchPostfix(Uri __result)
        {
            UriBuilder websocketUriBuilder = new UriBuilder(__result);
            string uriString = websocketUriBuilder.Uri.ToString();
            if (uriString.StartsWith((IsHttps) ? "wss" : "ws"))
            {
                UriBuilder backendUriBuilder = new UriBuilder(StayInTarkovHelperConstants.GetBackendUrl());
                websocketUriBuilder.Host = backendUriBuilder.Host;
                websocketUriBuilder.Port = backendUriBuilder.Port;
                if((uriString.StartsWith("wss") && !IsHttps) || (uriString.StartsWith("ws") && IsHttps)) websocketUriBuilder = new UriBuilder(uriString.Replace((IsHttps) ? "ws" : "wss", (IsHttps) ? "wss" : "ws"));
            }
            return websocketUriBuilder.Uri;
        }

    }
}
