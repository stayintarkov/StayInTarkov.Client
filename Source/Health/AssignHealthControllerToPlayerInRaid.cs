using EFT;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Assigns a HealthListener Instance to the Player in the Raid
    /// </summary>
    public class AssignHealthControllerToPlayerInRaid : ModulePatch
    {
        public static event Action<LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(Task __result, LocalPlayer __instance, Profile profile)
        {
            if (__instance is HideoutPlayer)
                return;

            if (OnPlayerInit != null)
                OnPlayerInit(__instance);

            await __result;

            var listener = HealthListener.Instance;
            if (profile?.Id.StartsWith("pmc") == true && __instance.IsYourPlayer)
            {
                //Logger.LogInfo($"Hooking up health listener to profile: {profile.Id}");
                listener.Init(__instance.HealthController, true);
                //Logger.LogInfo($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }
            else
            {
                //Logger.LogInfo($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");
            }

        }
    }
}
