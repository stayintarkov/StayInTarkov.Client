using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov
{
    public class WebSocketPatch : ModulePatch
    {
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
            return new Uri(__result.ToString().Replace("wss:", "ws:"));
        }

    }
}
