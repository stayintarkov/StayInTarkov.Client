using Comfort.Common;
using EFT;
using StayInTarkov.AkiSupport.Singleplayer.Utils.TraderServices;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.TraderServices
{
    public class LightKeeperServicesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld != null)
            {
                gameWorld.gameObject.AddComponent<LightKeeperServicesManager>();
            }
        }
    }
}
