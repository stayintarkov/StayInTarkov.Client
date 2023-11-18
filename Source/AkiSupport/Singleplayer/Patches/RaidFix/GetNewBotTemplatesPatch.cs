using EFT;
using SIT.Tarkov.Core;
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
        private static MethodInfo _getNewProfileMethod;

        static GetNewBotTemplatesPatch()
        {
            _ = nameof(IBotData.PrepareToLoadBackend);
            _ = nameof(BotsPresets.GetNewProfile);
            _ = nameof(PoolManager.LoadBundlesAndCreatePools);
            _ = nameof(JobPriority.General);
        }

        public GetNewBotTemplatesPatch()
        {
            var desiredType = typeof(LocalGameBotCreator);
            _getNewProfileMethod = desiredType
                .GetMethod(nameof(BotsPresets.GetNewProfile), BindingFlags.Instance | BindingFlags.NonPublic); // want the func with 2 params (protected + inherited from base)

            Logger.LogDebug($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {_getNewProfileMethod?.Name}");
        }

        /// <summary>
        /// Looking for CreateProfile()
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            var tMethod = typeof(LocalGameBotCreator).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(x => IsTargetMethod(x));

            Logger.LogInfo(tMethod.Name);
            GetLogger(typeof(GetNewBotTemplatesPatch)).LogInfo(tMethod.Name);
            return tMethod;
        }

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "data"
                && parameters[1].Name == "cancellationToken"
                && parameters[2].Name == "withDelete";
        }

        /// <summary>
        /// BotsPresets.GetNewProfile()
        /// </summary>
        [PatchPrefix]
        public static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, List<Profile> ___list_0, CreationData data, ref bool withDelete)
        {
            //PatchConstants.Logger.LogInfo("Prefix");
            //Logger.LogInfo("Prefix");
            //GetLogger(typeof(GetNewBotTemplatesPatch)).LogInfo("Prefix");

            try
            {
                // Force true to ensure bot profile is deleted after use
                _getNewProfileMethod.Invoke(__instance, new object[] { data, true });
            }
            catch (Exception e)
            {
                Logger.LogError($"getnewbot failed: {e.Message} {e.InnerException}");
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
            Logger.LogDebug($"{DateTime.Now:T} Loading bot {result.Info.Nickname} profile from server. role: {result.Info.Settings.Role} side: {result.Side}");

            return result;
        }
    }
}
