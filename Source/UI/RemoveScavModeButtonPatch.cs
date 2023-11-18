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
            pmcs.transform.localScale = new Vector3( (float)1.3, (float)1.3, (float)1.0);
            pmcs.transform.position = new Vector3((Screen.width / 2) - 293, (Screen.height / 2) - 130, 0);
        }
    }
}
