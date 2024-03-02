using System.Reflection;
using EFT;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    public class IsHostileToEverybodyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(ServerBotSettingsClass);
            var desiredMethod = desiredType.GetMethod("IsHostileToEverybody", BindingFlags.Public | BindingFlags.Static);

            return desiredMethod;
        }

        [PatchPrefix]
        private static bool PatchPrefix(WildSpawnType role, ref bool __result)
        {
            if (role == WildSpawnType.sptUsec || role == WildSpawnType.sptBear)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}

