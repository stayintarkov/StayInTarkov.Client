using Aki.Custom.Models;
using Diz.DependencyManager;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Build.Pipeline;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */

namespace StayInTarkov
{
    public class EasyBundlePatch : ModulePatch
    {
        static EasyBundlePatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BindableState<ELoadState>.Bind);
        }

        protected override MethodBase GetTargetMethod()
        {
            return EasyBundleHelper.Type.GetConstructors()[0];
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock)
        {
            var path = rootPath + key;
            var dependencyKeys = manifest.GetDirectDependencies(key) ?? new string[0];

            if (BundleManager.Bundles.TryGetValue(key, out BundleInfo bundle))
            {
                Logger.LogInfo(path);
                dependencyKeys = (dependencyKeys.Length > 0) ? dependencyKeys.Union(bundle.DependencyKeys).ToArray() : bundle.DependencyKeys;
                path = bundle.Path;
                Logger.LogInfo(path);

            }

            _ = new EasyBundleHelper(__instance)
            {
                Key = key,
                Path = path,
                KeyWithoutExtension = Path.GetFileNameWithoutExtension(key),
                DependencyKeys = dependencyKeys,
                LoadState = new BindableState<ELoadState>(ELoadState.Unloaded, null),
                BundleLock = bundleLock
            };
        }
    }
}
