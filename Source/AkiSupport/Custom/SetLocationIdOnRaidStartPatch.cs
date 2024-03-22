using EFT;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using System;
using static LocationSettingsClass;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/3.8.0/project/Aki.Custom/Patches/SetLocationIdOnRaidStartPatch.cs
    /// Modified by: KWJimWails. Converted to use StayInTarkovHelperConstants
    /// Local games do not set the locationId property like a network game does, `LocationId` is used by various bsg systems
    /// e.g. btr/lightkeeper services
    /// </summary>
    public class SetLocationIdOnRaidStartPatch : ModulePatch
    {
        private static PropertyInfo _locationProperty;

        protected override MethodBase GetTargetMethod()
        {
            Type localGameBaseType = StayInTarkovHelperConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType; // LocalGame

            // At this point, gameWorld.MainPlayer isn't set, so we need to use the LocalGame's `Location_0` property
            _locationProperty = localGameBaseType.GetProperties(StayInTarkovHelperConstants.PublicDeclaredFlags)
                .SingleCustom(x => x.PropertyType == typeof(Location));

            // Find the TimeAndWeatherSettings handling method
            var desiredMethod = localGameBaseType.GetMethods(StayInTarkovHelperConstants.PublicDeclaredFlags).SingleOrDefault(IsTargetMethod);

            Logger.LogDebug($"{GetType().Name} Type: {localGameBaseType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            // Find method_3(TimeAndWeatherSettings timeAndWeather)
            var parameters = mi.GetParameters();
            return (parameters.Length == 1 && parameters[0].ParameterType == typeof(TimeAndWeatherSettings));
        }

        [PatchPostfix]
        private static void PatchPostfix(AbstractGame __instance)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            // EFT.HideoutGame is an internal class, so we can't do static type checking :(
            if (__instance.GetType().Name.Contains("HideoutGame"))
            {
                return;
            }

            Location location = _locationProperty.GetValue(__instance) as Location;

            if (location == null)
            {
                Logger.LogError($"[SetLocationId] Failed to get location data");
                return;
            }

            gameWorld.LocationId = location.Id;

            Logger.LogInfo($"[SetLocationId] Set locationId to: {location.Id}");
        }
    }
}
