using Aki.Custom.Models;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.EssentialPatches;
using StayInTarkov.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
namespace StayInTarkov
{
    public static class BundleManager
    {
        private static ManualLogSource _logger;
        public static readonly ConcurrentDictionary<string, BundleItem> Bundles;
        public static string CachePath;

        static BundleManager()
        {
            _logger = Logger.CreateLogSource(nameof(BundleManager));
            Bundles = new ConcurrentDictionary<string, BundleItem>();
            CachePath = "user/cache/bundles/";
        }

        public static string GetBundlePath(BundleItem bundle)
        {
            return AkiBackendCommunication.IsLocal
                ? $"{bundle.ModPath}/bundles/{bundle.FileName}"
                : CachePath + bundle.FileName;
        }

        public static void GetBundles()
        {
            // get bundles
            var json = AkiBackendCommunication.Instance.GetJson("/singleplayer/bundles");
            var bundles = JsonConvert.DeserializeObject<BundleItem[]>(json);

            StayInTarkovHelperConstants.Logger.LogDebug($"[Bundle Manager] Bundles Json: {json}");

            // register bundles
            var toDownload = new ConcurrentBag<BundleItem>();
            var failDownload = new ConcurrentBag<BundleItem>();

            Parallel.ForEach(bundles, (bundle) =>
            {
                Bundles.TryAdd(bundle.FileName, bundle);

                if (ShouldReaquire(bundle))
                {
                    // mark for download
                    StayInTarkovHelperConstants.Logger.LogInfo($"Need Download Custom Bundle : {bundle.FileName}");
                    toDownload.Add(bundle);
                }
            });

            if (AkiBackendCommunication.IsLocal)
            {
                // loading from local mods
                _logger.LogInfo("CACHE: Loading all bundles from mods on disk.");
                return;
            }
            else
            {
                // download bundles
                // NOTE: assumes bundle keys to be unique
                foreach (var bundle in toDownload)
                {
                    // download bundle
                    var filepath = GetBundlePath(bundle);
                    StayInTarkovHelperConstants.Logger.LogInfo($"Start Downloading Custom Bundle : {bundle.FileName}");
                    try
                    {
                        // Using GetBundleData to download Bundle because the timeout period is 5 minutes.(For big bundles)
                        var data = AkiBackendCommunication.Instance.GetBundleData($"/files/bundle/{bundle.FileName}");
                        if (data != null && data.Length == 0)
                        {
                            StayInTarkovHelperConstants.Logger.LogError("Bundle received is 0 bytes. WTF!");
                        }
                        VFS.WriteFile(filepath, data);
                        StayInTarkovHelperConstants.Logger.LogInfo($"Writing Custom Bundle : {filepath}");
                    }
                    catch
                    {
                        StayInTarkovHelperConstants.Logger.LogError($"Download Custom Bundle {bundle.FileName} Failed, Try Again Later");
                        failDownload.Add(bundle);
                    }
                }

                foreach (var bundle in failDownload)
                {
                    // download bundle
                    var filepath = GetBundlePath(bundle);
                    StayInTarkovHelperConstants.Logger.LogInfo($"Start Re-downloading Custom Bundle : {bundle.FileName}");
                    try
                    {
                        // Using GetBundleData to download Bundle because the timeout period is 10 minutes.(For big bundles)
                        var data = AkiBackendCommunication.Instance.GetBundleData($"/files/bundle/{bundle.FileName}", 600000);
                        if (data != null && data.Length == 0)
                        {
                            StayInTarkovHelperConstants.Logger.LogError("Bundle received is 0 bytes. WTF!");
                        }
                        VFS.WriteFile(filepath, data);
                        StayInTarkovHelperConstants.Logger.LogInfo($"Writing Custom Bundle : {filepath}");
                    }
                    catch
                    {
                        StayInTarkovHelperConstants.Logger.LogError($"Download Custom Bundle Again {bundle.FileName} Failed");
                    }
                };
            }
        }

        private static bool ShouldReaquire(BundleItem bundle)
        {
            if (AkiBackendCommunication.IsLocal)
            {
                // only handle remote bundles
                return false;
            }

            // read cache
            var filepath = CachePath + bundle.FileName;

            if (VFS.Exists(filepath))
            {
                // calculate hash
                var data = VFS.ReadFile(filepath);
                var crc = Crc32.Compute(data);

                if (crc == bundle.Crc)
                {
                    // file is up-to-date
                    _logger.LogInfo($"CACHE: Loading locally {bundle.FileName}");
                    return false;
                }
                else
                {
                    // crc doesn't match, reaquire the file
                    _logger.LogInfo($"CACHE: Bundle is invalid, (re-)acquiring {bundle.FileName}");
                    return true;
                }
            }
            else
            {
                // file doesn't exist in cache
                _logger.LogInfo($"CACHE: Bundle is missing, (re-)acquiring {bundle.FileName}");
                return true;
            }
        }
    }
}
