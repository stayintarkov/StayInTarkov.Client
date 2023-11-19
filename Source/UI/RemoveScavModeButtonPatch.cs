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
        static void PatchPostfix(
            EFT.UI.Matchmaker.MatchMakerSideSelectionScreen __instance,
            DefaultUIButton ____savagesBigButton,
            UIAnimatedToggleSpawner ____savagesButton,
            DefaultUIButton ____pmcBigButton
            )
        {
            ____savagesBigButton.enabled = false;
            ____savagesButton.SpawnableToggle.enabled = false;
            ____savagesButton.gameObject.SetActive(false);
            //____savagesBigButton.transform.parent.gameObject.SetActive(false);

        }
    }
}
