using EFT;
using SIT.Tarkov.Core;
using StayInTarkov.Networking;
using System.Reflection;

namespace SIT.Core.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/QTEPatch.cs
    /// </summary>
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.StopWorkout));

        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            AkiBackendCommunication.Instance.PutJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }
            .ToJson());
        }
    }
}
