using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov
{

    /// <summary>
    /// Credit: BattlEyePatch from SPT-Aki https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/BattlEyePatch.cs
    /// </summary>
    public class AntiCheatPatch : ModulePatch
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


    /// <summary>
    /// SIT - A patch to remove FirstPassRun
    /// </summary>
    public class BattlEyePatchFirstPassRun : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "Run", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }

    /// <summary>
    /// SIT - A patch to remove FirstPassUpdate
    /// </summary>
    public class BattlEyePatchFirstPassUpdate : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "Update", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }

    /// <summary>
    /// SIT - A patch to test FirstPassReceivedPacket
    /// </summary>
    public class BattlEyePatchFirstPassReceivedPacket : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "ReceivedPacket", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            Logger.LogInfo("BattlEyePatchFirstPassReceivedPacket:PatchPrefix");
            return false;
        }
    }
}
