using EFT.UI;
using SIT.Tarkov.Core;
using System.Reflection;
using UnityEngine;


namespace StayInTarkov.UI
{
    /// <summary>
    /// Created by: Paulov
    /// </summary>
    public class RemoveScavModeButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.Matchmaker.MatchMakerSideSelectionScreen).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
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
            var pmccanvas = GameObject.Find("PMCPlayerMV");

            pmcs.transform.localScale = new Vector3(1.1f, 1.1f, 1.0f);
            pmcs.transform.position = new Vector3((Screen.width / 2) - (pmccanvas.RectTransform().rect.width / 2), (Screen.height / 2) - (Screen.height/10) , 0);
        }
    }
}
