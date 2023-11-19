using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using FilesChecker;
using HarmonyLib;
using System;
using SIT.Tarkov.Core;

namespace Aki.Core.Patches
{
    /// <summary>
    /// Credit: DataHandlerDebugPatch from SPT-Aki https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/
    /// </summary>
    public class DataHandlerDebugPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return StayInTarkovHelperConstants.EftTypes
                .Single(t => t.Name == "DataHandler")
                .GetMethod("method_5", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPrefix(ref string __result)
        {
            Console.WriteLine($"response json: ${__result}");
        }
    }
}