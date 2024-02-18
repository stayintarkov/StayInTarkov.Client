using EFT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid;


namespace StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode
{
    internal class ScavEncyclopediaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            if (RaidChangesUtil.IsScavRaid)
            {
                var scavProfile = StayInTarkovHelperConstants.BackEndSession.ProfileOfPet;
                var pmcProfile = StayInTarkovHelperConstants.BackEndSession.Profile;

                // Handle old profiles where the scav doesn't have an encyclopedia
                if (scavProfile.Encyclopedia == null)
                {
                    scavProfile.Encyclopedia = new Dictionary<string, bool>();
                }

                // Sync the PMC encyclopedia to the scav profile
                foreach (var item in pmcProfile.Encyclopedia.Where(item => !scavProfile.Encyclopedia.ContainsKey(item.Key)))
                {
                    scavProfile.Encyclopedia.Add(item.Key, item.Value);
                }

                // Auto examine any items the scav doesn't know that are in their inventory
                scavProfile.LearnAll();
            }
        }
    }
}