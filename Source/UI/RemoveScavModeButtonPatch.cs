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

            var pmcs = GameObject.Find("PMCs");
            var mmSSC = pmcs.transform.parent.gameObject;
            pmcs.transform.localPosition = new Vector3(-225, 500, 0);

        }
    }
}
