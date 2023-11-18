using EFT;
using SIT.Tarkov.Core;
using StayInTarkov;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.Core.FileChecker
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
