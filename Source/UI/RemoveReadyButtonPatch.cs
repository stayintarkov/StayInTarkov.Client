using EFT.UI;
using EFT.UI.Matchmaker;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.UI
{
    public class RemoveReadyButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(MatchMakerSelectionLocationScreen), "method_7");
        }

        [PatchPostfix]
        static void PatchPostfix()
        {
            var readyButton = GameObject.Find("ReadyButton");

            if (readyButton != null)
            {
                DefaultUIButton uiButton = readyButton.GetComponent<DefaultUIButton>();

                if (uiButton != null)
                {
                    if (uiButton.Interactable == true)
                    {
                        uiButton.Interactable = false;
                        uiButton.SetDisabledTooltip("Disabled with SIT");
                    }
                }
            }
        }
    }
}