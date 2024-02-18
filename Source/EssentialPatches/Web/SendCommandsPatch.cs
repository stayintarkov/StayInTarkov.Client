using System.Reflection;

namespace StayInTarkov.EssentialPatches.Web
{
    /// <summary>
    /// Created by: Paulov
    /// Description: This patch removes the wait to push changes from Inventory
    /// </summary>
    internal class SendCommandsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(TradingBackend1), "TrySendCommands");
        }

        [PatchPrefix]
        public static bool Prefix(

            ref float ___float_0
            )
        {
            ___float_0 = 0;
            return true;
        }

    }
}
