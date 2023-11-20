using StayInTarkov;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Aki.Core.Patches
{
    /// <summary>
    /// Credit: BattlEyePatch from SPT-Aki https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/BattlEyePatch.cs
    /// </summary>
    /// WE ARE NOT PATHCING WITH THIS - TO BE REMOVED
    public class BattlEyePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "RunValidation";
            var flags = BindingFlags.Public | BindingFlags.Instance;

            return StayInTarkovHelperConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, ref bool ___bool_0)
        {
            ___bool_0 = true;
            __result = Task.CompletedTask;
            return false;
        }
    }
}
