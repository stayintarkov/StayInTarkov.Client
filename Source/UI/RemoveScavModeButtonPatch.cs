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
            var mmSSC = pmcs.transform.parent.gameObject;
            pmcs.transform.position = new Vector3(mmSSC.transform.position.x / 1.305f, pmcs.transform.position.y * 0.75f, 0);
        }
    }
}
