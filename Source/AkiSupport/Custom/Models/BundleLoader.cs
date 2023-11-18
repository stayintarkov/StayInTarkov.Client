using Comfort.Common;
using EFT;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aki.Custom.Models
{
    public struct BundleLoader
    {
        Profile Profile;
        TaskScheduler TaskScheduler { get; }

        public BundleLoader(TaskScheduler taskScheduler)
        {
            Profile = null;
            TaskScheduler = taskScheduler;
        }

        public Task<Profile> LoadBundles(Task<Profile> task)
        {
            Profile = task.Result;

            var loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                PoolManager.PoolsCategory.Raid,
                PoolManager.AssemblyType.Local,
                Profile.GetAllPrefabPaths(false).ToArray(),
                JobPriority.General,
                null,
                default(CancellationToken));

            return loadTask.ContinueWith(GetProfile, TaskScheduler);
        }

        private Profile GetProfile(Task task)
        {
            return Profile;
        }
    }
}