using Diz.Jobs;
using Diz.Resources;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using DependencyGraph = DependencyGraph<IEasyBundle>;

namespace Aki.Custom.Patches
{
    public class EasyAssetsPatch : ModulePatch
    {
        private static readonly FieldInfo _manifestField;
        private static readonly FieldInfo _bundlesField;
        private static readonly PropertyInfo _systemProperty;

        static EasyAssetsPatch()
        {
            var type = typeof(EasyAssets);

            _manifestField = type.GetField(nameof(EasyAssets.Manifest));
            if (_manifestField == null)
                Logger.LogError("_manifestField is NULL");

            //Logger.LogDebug(EasyBundleHelper.Type.Name.ToLowerInvariant());

            var arrayEBH = Array.CreateInstance(EasyBundleHelper.Type, 0);

            _bundlesField = ReflectionHelpers.GetFieldFromTypeByFieldType(type, arrayEBH.GetType(), false);// type.GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);
            if (_bundlesField == null)
                Logger.LogError("_bundlesField is NULL");
            // DependencyGraph<IEasyBundle>
            _systemProperty = type.GetProperty("System");
            if (_systemProperty == null)
                Logger.LogError("_systemProperty is NULL");
        }

        public EasyAssetsPatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BundleLock.MaxConcurrentOperations);
            _ = nameof(DependencyGraph.GetDefaultNode);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EasyAssets).GetMethods(StayInTarkovHelperConstants.PrivateFlags).Single(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 6
                    && parameters[0].Name == "bundleLock"
                    && parameters[1].Name == "defaultKey"
                    && parameters[4].Name == "shouldExclude");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, EasyAssets __instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
            string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        {
            __result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
            return false;
        }

        private static async Task Init(EasyAssets instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
                                      string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            // platform manifest
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = path + platformName;
            var manifest = (File.Exists(filepath)) ? await GetManifestBundle(filepath) : await GetManifestJson(filepath);

            var allAssetBundles = manifest.GetAllAssetBundles();

            if (allAssetBundles == null)
                return;

            if (BundleManager.Bundles.Keys == null)
                return;

            // load bundles
            var bundleNames = manifest.GetAllAssetBundles().Union(BundleManager.Bundles.Keys).ToArray();

            if (bundleNames == null)
                return;

            var bundles = (IEasyBundle[])Array.CreateInstance(EasyBundleHelper.Type, bundleNames.Length);

            if (bundles == null)
                return;

            if (bundleLock == null)
            {
                bundleLock = new BundleLock(int.MaxValue);
            }

            for (var i = 0; i < bundleNames.Length; i++)
            {
                //Logger.LogDebug(bundleNames[i]);

                bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[]
                    {
                        bundleNames[i],
                        path,
                        manifest,
                        bundleLock,
                        bundleCheck
                    });

                if (bundles[i] == null)
                    continue;

                await JobScheduler.Yield(EJobPriority.Immediate);
            }

            if(_manifestField != null)
                _manifestField.SetValue(instance, manifest);
            if(_bundlesField != null)
                _bundlesField.SetValue(instance, bundles);
            if (_systemProperty != null)
                _systemProperty.SetValue(instance, new DependencyGraph(bundles, defaultKey, shouldExclude));
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestBundle(string filepath)
        {
            Logger.LogDebug("GetManifestBundle:Start");
            var manifestLoading = AssetBundle.LoadFromFileAsync(filepath);
            await manifestLoading.Await();

            var assetBundle = manifestLoading.assetBundle;
            var assetLoading = assetBundle.LoadAllAssetsAsync();
            await assetLoading.Await();

            Logger.LogDebug("GetManifestBundle:End");

            return (CompatibilityAssetBundleManifest)assetLoading.allAssets[0];
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestJson(string filepath)
        {
            Logger.LogDebug("GetManifestJson:Start");

            var text = string.Empty;

            using (var reader = File.OpenText($"{filepath}.json"))
            {
                text = await reader.ReadToEndAsync();
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, Models.BundleItem>>(text).ToDictionary(GetPairKey, GetPairValue);
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(data);

            Logger.LogDebug("GetManifestJson:End");

            return manifest;
        }

        public static string GetPairKey(KeyValuePair<string, Models.BundleItem> x)
        {
            return x.Key;
        }

        public static BundleDetails GetPairValue(KeyValuePair<string, Models.BundleItem> x)
        {
            return new BundleDetails
            {
                FileName = x.Value.FileName,
                Crc = x.Value.Crc,
                Dependencies = x.Value.Dependencies
            };
        }
    }
}
