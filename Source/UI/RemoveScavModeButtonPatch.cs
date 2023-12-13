using EFT.UI;
using System.Reflection;
using UnityEngine;


namespace StayInTarkov.UI
{
    public class RemoveScavModeButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.UI.Matchmaker.MatchMakerSideSelectionScreen), "Awake");
        }

        [PatchPostfix]
        static void PatchPostfix()
        {
            var selecttxt = GameObject.Find("SideSelectionCaption");
            selecttxt.active = false;

            var savages = GameObject.Find("Savage");
            savages.active = false;

            var pmcdesc = GameObject.Find("Description");
            pmcdesc.active = false;

            Logger.LogInfo(Screen.width);
            Logger.LogInfo(Screen.width / 2);

            var pmcs = GameObject.Find("PMCs");
            var mmSSC = pmcs.transform.parent.gameObject;
            Logger.LogInfo(pmcs.transform.position.x);
            Logger.LogInfo(mmSSC.transform.position.x);
            pmcs.transform.position = new Vector3(Screen.width * 0.39f, pmcs.transform.position.y * 0.75f, 0);
           
        }
    }
}
