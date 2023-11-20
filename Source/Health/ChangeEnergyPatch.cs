using System.Linq;
using System.Reflection;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Detect changes in "Energy" and pass those changes to the HealthListener
    /// </summary>
    internal class ChangeEnergyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return StayInTarkovHelperConstants.EftTypes
                .LastOrDefault(x => x.GetMethod("ChangeEnergy", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("ChangeEnergy", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , float value
            )
        {
            if (__instance == HealthListener.Instance.MyHealthController)
            {
                //Logger.LogInfo("ChangeEnergyPatch:PatchPostfix:Change on my Health Controller: " + value);
                if (HealthListener.Instance.CurrentHealth.Energy + value > 0)
                    HealthListener.Instance.CurrentHealth.Energy += value;
            }
        }
    }
}
