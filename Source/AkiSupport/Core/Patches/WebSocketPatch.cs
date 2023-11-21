using StayInTarkov;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Core.Patches
{
    /// <summary>
    /// Credit: WebSocketPatch from SPT-Aki https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/
    /// </summary>
    /// WE ARE NOT PATHCING WITH THIS - TO BE REMOVED
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
