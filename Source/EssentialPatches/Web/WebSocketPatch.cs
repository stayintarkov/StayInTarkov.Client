using System;
using System.Linq;
using System.Reflection;

namespace SIT.Tarkov.Core
{
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetInterface = StayInTarkovHelperConstants.EftTypes.Single(x => x == typeof(IConnectionHandler) && x.IsInterface);
            var typeThatMatches = StayInTarkovHelperConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            return typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.ReturnType == typeof(Uri));
        }

        [PatchPostfix]
        private static Uri PatchPostfix(Uri __instance)
        {
            return new Uri(__instance.ToString().Replace("wss:", "ws:"));
        }
    }

}
