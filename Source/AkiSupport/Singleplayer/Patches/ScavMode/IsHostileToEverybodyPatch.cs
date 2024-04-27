using System.Reflection;
using EFT;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    public class IsHostileToEverybodyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(BotSettingsRepoClass);
            var desiredMethod = desiredType.GetMethod("IsHostileToEverybody", BindingFlags.Public | BindingFlags.Static);

            return desiredMethod;
        }

        [PatchPrefix]
        private static bool PatchPrefix(WildSpawnType role, ref bool __result)
        {
            if ((int)role == 49 || (int)role == 50)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}

