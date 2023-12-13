using EFT;
using StayInTarkov;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Healing
{
    /// <summary>
    /// We need to alter Class1049.smethod_0().
    /// Set the passed in ERaidMode to online, this ensures the heal screen shows.
    /// It cannot be changed in the calling method as doing so causes the post-raid exp display to remain at 0
    /// </summary>
    public class PostRaidHealScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Class1049.smethod_0 as of 18969
            //internal static Class1049 smethod_0(GInterface29 backend, string profileId, Profile savageProfile, LocationSettingsClass.GClass1097 location, ExitStatus exitStatus, TimeSpan exitTime, ERaidMode raidMode)
            MethodInfo foundMethod = null;
            Type foundType = null;
            foreach (var t in StayInTarkovHelperConstants.EftTypes)
            {
                var methods = ReflectionHelpers.GetAllMethodsForType(t);
                foundMethod = methods.FirstOrDefault(m => IsTargetMethod(m));
                if (foundMethod != null)
                {
                    foundType = t;
                    break;
                }
            }

            Logger.LogDebug($"{this.GetType().Name} Type: {foundType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {foundMethod?.Name}");

            return foundMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 7
                && parameters[0].Name == "session"
                && parameters[1].Name == "profileId"
                && parameters[2].Name == "savageProfile"
                && parameters[3].Name == "location"
                && parameters[4].Name == "exitStatus"
                && parameters[5].Name == "exitTime"
                && parameters[6].Name == "raidMode";
        }

        [PatchPrefix]
        private static bool PatchPrefix(TarkovApplication __instance, ref ERaidMode raidMode)
        {
            raidMode = ERaidMode.Online;

            return true; // Perform original method
        }
    }
}