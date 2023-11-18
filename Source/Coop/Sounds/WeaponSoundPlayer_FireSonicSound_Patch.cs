using SIT.Tarkov.Core;
using StayInTarkov;
using System.Reflection;

namespace SIT.Core.Coop.Sounds
{
    internal class WeaponSoundPlayer_FireSonicSound_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "FireSonicSound");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
