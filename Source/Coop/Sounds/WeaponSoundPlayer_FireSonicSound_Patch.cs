using System.Reflection;

namespace StayInTarkov.Coop.Sounds
{
    public class WeaponSoundPlayer_FireSonicSound_Patch : ModulePatch
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
