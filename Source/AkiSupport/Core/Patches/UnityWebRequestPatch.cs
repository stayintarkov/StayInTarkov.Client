using StayInTarkov;
using System.Reflection;
using UnityEngine.Networking;

namespace Aki.Core.Patches
{
    /// <summary>
    /// Credit: UnityWebRequestPatch from SPT-Aki https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/
    /// </summary>
    public class UnityWebRequestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(UnityWebRequestTexture).GetMethod(nameof(UnityWebRequestTexture.GetTexture), new[] { typeof(string) });
        }

        [PatchPostfix]
        private static void PatchPostfix(UnityWebRequest __result)
        {
            __result.certificateHandler = new FakeCertificateHandler();
            __result.disposeCertificateHandlerOnDispose = true;
            __result.timeout = 15000;
        }

        internal class FakeCertificateHandler : UnityEngine.Networking.CertificateHandler
        {
            public override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}
