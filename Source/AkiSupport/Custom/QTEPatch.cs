using EFT;
using StayInTarkov.Networking;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/QTEPatch.cs
    /// Edit for SIT: Modified as it is not sending the updated skills, IDK why; ExitQte gets only called after all QTEs are done, whether by broken arm or finishing normally
    /// Notice by Editor: not sure why this behavior differs, but the edit makes it usable for SIT
    /// </summary>
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.ExitQte)); //AKI uses StopWorkout

        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            AkiBackendCommunication.Instance.PostJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }
            .ToJson());
        }
    }
}