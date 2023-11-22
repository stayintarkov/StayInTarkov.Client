using StayInTarkov;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link:  https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/LocationLootCacheBustingPatch.cs
    /// </summary>
    public class LocationLootCacheBustingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = StayInTarkovHelperConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType; // BaseLocalGame
            var desiredMethod = ReflectionHelpers.GetAllMethodsForType(desiredType).Single(x => IsTargetMethod(x)); // method_6

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "backendUrl"
                && parameters[1].Name == "locationId"
                && parameters[2].Name == "variantId";
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false; // skip original
        }
    }
}
