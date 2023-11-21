using EFT;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.SITFixes
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Fixes an error on RAID start for the IsFollower method.
    /// </summary>
    internal class BotSettingsRepoClassIsFollowerFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = StayInTarkovHelperConstants.EftTypes.First(x => x.GetMethods().Any(y => y.Name == "IsFollower" && y.GetParameters().Length == 1 && y.GetParameters()[0].ParameterType == typeof(WildSpawnType)));
            return ReflectionHelpers.GetMethodForType(t, "IsFollower");
        }


        [PatchPrefix]
        public static bool Prefix(ref bool __result, EFT.WildSpawnType role)
        {
            __result = false;
            return false;
        }

        [PatchPostfix]
        public static void Postfix(ref bool __result, EFT.WildSpawnType role)
        {
            __result = false;
        }
    }
}
