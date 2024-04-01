using Aki.Custom.Models;
using Aki.Custom.Utils;
using Diz.Jobs;
using Diz.Resources;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
        private static readonly FieldInfo _bundlesField;

        static EasyAssetsPatch()
        {
            var type = typeof(EasyAssets);
            var arrayEBH = Array.CreateInstance(EasyBundleHelper.Type, 0);
            _bundlesField = ReflectionHelpers.GetFieldFromTypeByFieldType(type, arrayEBH.GetType(), false);// type.GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);

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
            return typeof(EasyAssets).GetMethods(StayInTarkovHelperConstants.PublicDeclaredFlags).Single(IsTargetMethod);
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

        private static async Task Init(EasyAssets instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath, string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            // platform manifest
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = path + platformName;
            var jsonfile = filepath + ".json";
            var manifest = File.Exists(jsonfile)
                ? await GetManifestJson(jsonfile)
                : await GetManifestBundle(filepath);

            // create bundles array from obfuscated type
            var bundleNames = manifest.GetAllAssetBundles()
                .Union(BundleManager.Bundles.Keys)
                .ToArray();

            // create bundle lock
            if (bundleLock == null)
            {
                bundleLock = new BundleLock(int.MaxValue);
            }

            // create bundle of obfuscated type
            var bundles = (IEasyBundle[])Array.CreateInstance(EasyBundleHelper.Type, bundleNames.Length);

            for (var i = 0; i < bundleNames.Length; i++)
            {
                bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[]
                {
                    bundleNames[i],
                    path,
                    manifest,
                    bundleLock,
                    bundleCheck
                });

                await JobScheduler.Yield(EJobPriority.Immediate);
            }

            // create dependency graph
            instance.Manifest = manifest;
            _bundlesField.SetValue(instance, bundles);
            instance.System = new DependencyGraph(bundles, defaultKey, shouldExclude);
        }

        // NOTE: used by:
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/cubemaps
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/defaultmaterial
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/dissonancesetup
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/Doge
        // - EscapeFromTarkov_Data/StreamingAssets/Windows/shaders
        private static async Task<CompatibilityAssetBundleManifest> GetManifestBundle(string filepath)
        {
            var manifestLoading = AssetBundle.LoadFromFileAsync(filepath);
            await manifestLoading.Await();

            var assetBundle = manifestLoading.assetBundle;
            var assetLoading = assetBundle.LoadAllAssetsAsync();
            await assetLoading.Await();

            return (CompatibilityAssetBundleManifest)assetLoading.allAssets[0];
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestJson(string filepath)
        {
            var text = VFS.ReadTextFile(filepath);

            /* we cannot parse directly as <string, BundleDetails>, because...
                    [Error  : Unity Log] JsonSerializationException: Expected string when reading UnityEngine.Hash128 type, got 'StartObject' <>. Path '['assets/content/weapons/animations/simple_animations.bundle'].Hash', line 1, position 176.
               ...so we need to first convert it to a slimmed-down type (BundleItem), then convert back to BundleDetails.
            */
            var raw = JsonConvert.DeserializeObject<Dictionary<string, BundleItem>>(text);
            var converted = raw.ToDictionary(GetPairKey, GetPairValue);

            // initialize manifest
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(converted);

            return manifest;
        }

        public static string GetPairKey(KeyValuePair<string, BundleItem> x)
        {
            return x.Key;
        }

        public static BundleDetails GetPairValue(KeyValuePair<string, BundleItem> x)
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