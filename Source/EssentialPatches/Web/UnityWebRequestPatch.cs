using System.Reflection;
using UnityEngine.Networking;

namespace StayInTarkov
{ 
    /// WE ARE NOT PATHCING WITH THIS - TO BE REMOVED
    public class UnityWebRequestPatch : ModulePatch
    {
        private static UnityEngine.Networking.CertificateHandler _certificateHandler = new FakeCertificateHandler();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(UnityWebRequestTexture).GetMethod(nameof(UnityWebRequestTexture.GetTexture), new[] { typeof(string) });
        }

        [PatchPostfix]
        private static void PatchPostfix(UnityWebRequest __result)
        {
            __result.certificateHandler = _certificateHandler;
            __result.disposeCertificateHandlerOnDispose = false;
            __result.timeout = 1000;
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
