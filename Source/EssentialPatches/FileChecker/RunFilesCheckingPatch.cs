using EFT;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.FileChecker
{
    /// <summary>
    /// SIT - RunFilesCheckingPatch
    /// </summary>
    internal class RunFilesCheckingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(CommonClientApplication<ISession>), "RunFilesChecking");
        }

        [PatchPrefix]
        public static bool Prepatch()
        {
            return false;
        }

        [PatchPostfix]
        public static async Task Postpatch(
            Task __result
            )
        {
            await Task.Yield();
            __result = Task.CompletedTask;
        }
    }
}
