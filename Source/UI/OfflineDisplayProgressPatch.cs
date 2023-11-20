using EFT;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.UI
{
    internal class OfflineDisplayProgressPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            foreach (var method in ReflectionHelpers.GetAllMethodsForType(StayInTarkovHelperConstants.EftTypes.Single(x => x == typeof(TarkovApplication))))
            {
                if (method.Name.StartsWith("method") &&
                    method.GetParameters().Length >= 5 &&
                    method.GetParameters()[0].Name == "profileId" &&
                    method.GetParameters()[1].Name == "savageProfile" &&
                    method.GetParameters()[2].Name == "location" &&
                    method.GetParameters().Any(x => x.Name == "exitStatus") &&
                    method.GetParameters().Any(x => x.Name == "exitTime")
                    )
                {
                    Logger.LogInfo("OfflineDisplayProgressPatch:Method Name:" + method.Name);
                    return method;
                }
            }
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineDisplayProgressPatch::Method is not found!");

            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix(
            ref RaidSettings ____raidSettings
            )
        {
            ____raidSettings.RaidMode = ERaidMode.Local;
            return true;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            ref RaidSettings ____raidSettings
            )
        {
            ____raidSettings.RaidMode = ERaidMode.Local;
        }
    }
}
