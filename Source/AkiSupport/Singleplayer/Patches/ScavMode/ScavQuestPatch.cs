using System.Linq;
using System.Reflection;
using EFT.UI.Matchmaker;
using HarmonyLib;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    /// <summary>
    /// Copy over scav-only quests from PMC profile to scav profile on pre-raid screen
    /// Allows scavs to see and complete quests
    /// </summary>
    public class ScavQuestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(MatchmakerOfflineRaidScreen))
                .SingleCustom(m => m.Name == nameof(MatchmakerOfflineRaidScreen.Show) && m.GetParameters().Length == 1);
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var pmcProfile = StayInTarkovHelperConstants.BackEndSession.Profile;
            var scavProfile = StayInTarkovHelperConstants.BackEndSession.ProfileOfPet;

            // Iterate over all quests on pmc that are flagged as being for scavs
            foreach (var quest in pmcProfile.QuestsData.Where(x => x.Template?.PlayerGroup == EFT.EPlayerGroup.Scav))
            {
                // If quest doesnt exist in scav, add it
                if (!scavProfile.QuestsData.Any(x => x.Id == quest.Id))
                {
                    scavProfile.QuestsData.Add(quest);
                }
            }
        }
    }
}