using System;
using System.Linq;
using EFT;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    public class ScavProfileLoadCoopPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(TarkovApplication);
            var desiredMethod = Array.Find(desiredType.GetMethods(StayInTarkovHelperConstants.PublicDeclaredFlags), IsTargetMethod);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 5
                    && parameters[0].Name == "profileId"
                    && parameters[1].Name == "savageProfile"
                    && parameters[2].Name == "location"
                    && parameters[3].Name == "result"
                    && parameters[4].Name == "timeHasComeScreenController");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref string profileId, Profile savageProfile, RaidSettings ____raidSettings)
        {
            if (____raidSettings.IsPmc)
                return true;
            profileId = savageProfile.Id;
            
            return true; // Always do original method
        }
    }
}