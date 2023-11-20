using System.Reflection;

namespace StayInTarkov
{
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
