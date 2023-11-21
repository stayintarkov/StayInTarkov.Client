using FilesChecker;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov
{
    /// <summary>
    /// Credit: SPT-Aki ConsistencySinglePatch - https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/ConsistencySinglePatch.cs
    /// </summary>
    public class ConsistencySinglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return StayInTarkovHelperConstants.FilesCheckerTypes.Single(x => x.Name == "ConsistencyController")
                .GetMethods().Single(x => x.Name == "EnsureConsistencySingle" && x.ReturnType == typeof(Task<ICheckResult>));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref object __result)
        {
            __result = Task.FromResult<ICheckResult>(new FakeFileCheckerResult());
            return false;
        }
    }
}
