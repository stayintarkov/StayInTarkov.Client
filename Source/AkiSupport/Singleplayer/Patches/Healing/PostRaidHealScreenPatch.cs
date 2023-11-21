using EFT;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Healing
{
    public class PostRaidHealScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(ProfileChangeHandler);
            return ReflectionHelpers.GetMethodForType(desiredType, "smethod_0");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref ERaidMode raidMode)
        {
            raidMode = ERaidMode.Online;

            return true;
        }
    }
}
