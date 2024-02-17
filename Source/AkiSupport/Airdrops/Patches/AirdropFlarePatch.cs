using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.InventoryLogic;
using EFT.PrefabSettings;
using StayInTarkov;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Airdrops.Patches
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Airdrops/Patches
    /// Paulov: Modified to use SITAirdropsManager
    /// </summary>
    public class AirdropFlarePatch : ModulePatch
    {
        private static readonly string[] _usableFlares = { "624c09cfbc2e27219346d955", "62389ba9a63f32501b1b4451" };

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(FlareCartridge), nameof(FlareCartridge.Init), false, true);
        }

        [PatchPostfix]
        private static void PatchPostfix(FlareCartridgeSettings flareCartridgeSettings, IPlayer player, BulletClass flareCartridge, Weapon weapon)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points && _usableFlares.Any(x => x == flareCartridge.Template._id))
            {
                gameWorld.gameObject.AddComponent<SITAirdropsManager>().isFlareDrop = true;
            }
        }
    }
}