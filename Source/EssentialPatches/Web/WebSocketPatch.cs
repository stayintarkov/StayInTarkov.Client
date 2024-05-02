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
            string url = __result.ToString();
            if (url.StartsWith((IsHttps) ? "wss:" : "ws:"))
            {
                int firstSlash = url.IndexOf("/", 8);
                string newWs = url.Substring(firstSlash, url.Length - firstSlash);
                string backendUrl = StayInTarkovHelperConstants.GetBackendUrl();
                backendUrl = backendUrl.Replace((IsHttps) ? "https" : "http", (IsHttps) ? "wss" : "ws");
                newWs = backendUrl + newWs;
                url = newWs;
            }
            return new Uri(url);
        }

    }
}
