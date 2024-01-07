using Comfort.Common;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// BSG have disabled notifications for local raids, set updateAchievements in the achievement controller to always be true
/// This enables the achievement notifications and the client to save completed achievement data into profile.Achievements
/// </summary>


namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Progression
{
    public class MidRaidAchievementChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AchievementControllerClass).GetConstructors()[0];
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool updateAchievements)
        {
            updateAchievements = true;
            return true;
        }
    }
}
