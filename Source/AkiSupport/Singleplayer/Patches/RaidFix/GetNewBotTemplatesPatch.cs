using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.RaidFix
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Patches/RaidFix/GetNewBotTemplatesPatch.cs
    /// </summary>
    public class GetNewBotTemplatesPatch : ModulePatch
    {
        static GetNewBotTemplatesPatch()
        {
            _ = nameof(IGetProfileData.PrepareToLoadBackend);
            _ = nameof(BotsPresets.GetNewProfile);
            _ = nameof(PoolManager.LoadBundlesAndCreatePools);
            _ = nameof(JobPriority.General);
        }

        /// <summary>
        /// Looking for CreateProfile()
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(BotsPresets), nameof(BotsPresets.CreateProfile));
        }

        // Unused, but left here in case patch breaks and finding the intended method is difficult
        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 3
                && parameters[0].Name == "data"
                && parameters[1].Name == "cancellationToken"
                && parameters[2].Name == "withDelete");
        }

        /// <summary>
        /// BotsPresets.GetNewProfile()
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, List<Profile> ___list_0, Data1 data, ref bool withDelete)
        {
            /*
                When client wants new bot and GetNewProfile() return null (if not more available templates or they don't satisfy by Role and Difficulty condition)
                then client gets new piece of WaveInfo collection (with Limit = 30 by default) and make request to server
                but use only first value in response (this creates a lot of garbage and cause freezes)
                after patch we request only 1 template from server along with other patches this one causes to call data.PrepareToLoadBackend(1) gets the result with required role and difficulty:
                new[] { new WaveInfo() { Limit = 1, Role = role, Difficulty = difficulty } }
                then perform request to server and get only first value of resulting single element collection
            */

            try
            {
                // Force true to ensure bot profile is deleted after use
                __instance.GetNewProfile(data, true);
            }
            catch (Exception e)
            {
                Logger.LogDebug($"GetNewBotTemplatesPatch() getNewProfile() failed: {e.Message} {e.InnerException}");
                throw;
            }

            // Load from server
            var source = data.PrepareToLoadBackend(1).Take(1).ToList();

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskAwaiter = (Task<Profile>)null;
            taskAwaiter = StayInTarkovHelperConstants.BackEndSession.LoadBots(source).ContinueWith(GetFirstResult, taskScheduler);

            // Load bundles for bot profile
            var continuation = new Aki.Custom.Models.BundleLoader(taskScheduler);
            __result = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();

            return false;
        }

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            var result = task.Result[0];
            Logger.LogInfo($"{DateTime.Now:T} Loading bot: {result.Info.Nickname} profile from server. Role: {result.Info.Settings.Role} Side: {result.Side}");

            return result;
        }
    }
}
