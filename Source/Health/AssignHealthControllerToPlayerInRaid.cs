using EFT;
using HarmonyLib;
using StayInTarkov.Networking;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Assigns a HealthListener Instance to the Player in the Raid
    /// </summary>
    internal class AssignHealthControllerToPlayerInRaid : ModulePatch
    {
        public static event Action<LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalPlayer), nameof(LocalPlayer.Init));
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

            if (profile?.Id.Equals(AkiBackendCommunication.Instance.ProfileId, StringComparison.InvariantCultureIgnoreCase) ?? false && __instance.IsYourPlayer)
            {
                Logger.LogDebug($"Hooking up health listener to profile: {profile.Id}");
                listener.Init(__instance.HealthController, true);
                Logger.LogDebug($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }
            else
            {
                Logger.LogDebug($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");
            }

        }
    }
}
