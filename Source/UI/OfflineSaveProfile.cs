using Comfort.Common;
using EFT;
using StayInTarkov.AkiSupport.Singleplayer.Models.Healing;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Health;
using StayInTarkov.Networking;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.UI
{
    /// <summary>
    /// Original by SPT-Aki team https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/Progression/OfflineSaveProfilePatch.cs
    /// Modified by Paulov to suit SIT.
    /// Description: OfflineSaveProfile runs when the RAID ends and saves the Active Profile to the Backend via a custom web call.
    /// </summary>
    public class OfflineSaveProfile : ModulePatch
    {
        public static MethodInfo GetMethod()
        {
            foreach (var method in ReflectionHelpers.GetAllMethodsForType(typeof(TarkovApplication)))
            {
                if (method.Name.StartsWith("method") &&
                    method.GetParameters().Length >= 3 &&
                    method.GetParameters()[0].Name == "profileId" &&
                    method.GetParameters()[1].Name == "savageProfile" &&
                    method.GetParameters()[2].Name == "location" &&
                    method.GetParameters().Any(x => x.Name == "result") &&
                    method.GetParameters()[method.GetParameters().Length - 1].Name == "timeHasComeScreenController"
                    )
                {
                    //Logger.Log(BepInEx.Logging.LogLevel.Info, method.Name);
                    return method;
                }
            }
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineSaveProfile::Method is not found!");

            return null;
        }

        protected override MethodBase GetTargetMethod()
        {
            return GetMethod();
        }

        [PatchPrefix]
        public static bool PatchPrefix(string profileId, RaidSettings ____raidSettings, TarkovApplication __instance, Result<ExitStatus, TimeSpan, object> result)
        {
            Logger.LogInfo("Saving Profile...");

            // Get scav or pmc profile based on IsScav value
            var profile = ____raidSettings.IsScav
                ? __instance.GetClientBackEndSession().ProfileOfPet
                : __instance.GetClientBackEndSession().Profile;

            var currentHealth = HealthListener.Instance.CurrentHealth;

            // Set PMCs half health for heal screen
            if (!____raidSettings.IsScav)
            {
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.Head);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.Chest);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.Stomach);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.LeftArm);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.RightArm);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.LeftLeg);
                HealthListener.HealHalfHealth(HealthListener.Instance.MyHealthController, currentHealth.Health, EBodyPart.RightLeg);
                currentHealth = HealthListener.Instance.CurrentHealth;
            }

            SaveProfileProgress(result.Value0, profile, currentHealth, ____raidSettings.IsScav);


            var coopGC = SITGameComponent.GetCoopGameComponent();
            if (coopGC != null)
            {
                UnityEngine.Object.Destroy(coopGC);
            }

            HealthListener.Instance.MyHealthController = null;
            return true;
        }

        public static void SaveProfileProgress(ExitStatus exitStatus, Profile profileData, PlayerHealth currentHealth, bool isPlayerScav)
        {
            // "Disconnecting" from your game in Single Player shouldn't result in losing your gear. This is stupid.
            if (exitStatus == ExitStatus.Left)
                exitStatus = ExitStatus.Runner;

            // TODO: Remove uneccessary data
            //var clonedProfile = profileData.Clone();
            //clonedProfile.Encyclopedia = null;
            //clonedProfile.Hideout = null;
            //clonedProfile.Notes = null;
            //clonedProfile.RagfairInfo = null;
            //clonedProfile.Skills = null;
            //clonedProfile.TradersInfo = null;
            //clonedProfile.QuestsData = null;
            //clonedProfile.UnlockedRecipeInfo = null;
            //clonedProfile.WishList = null;

            SaveProfileRequest request = new()
            {
                exit = exitStatus.ToString().ToLower(),
                profile = profileData,
                health = currentHealth,
                isPlayerScav = isPlayerScav
            };

            var convertedJson = request.SITToJson();
            //Logger.LogDebug("SaveProfileProgress =====================================================");
            //Logger.LogDebug(convertedJson);
            AkiBackendCommunication.Instance.PostJson("/raid/profile/save", convertedJson);
            //_ = AkiBackendCommunication.Instance.PostJsonAsync("/raid/profile/save", convertedJson, timeout: 10 * 1000, debug: false);


            //Request.Instance.PostJson("/raid/profile/save", convertedJson, timeout: 60 * 1000, debug: true);
        }

        public class SaveProfileRequest
        {
            public string exit { get; set; }
            public Profile profile { get; set; }
            public bool isPlayerScav { get; set; }
            public object health { get; set; }
        }
    }
}
