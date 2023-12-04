using Comfort.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadMag";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, MagazineClass magazine, GridItemAddress gridItemAddress, EFT.Player ____player)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(magazine);

                // This should probably be GClass2156

                coopPlayer.AddCommand(new GClass2152()
                {
                    MagTypeCurrent = __instance.Weapon.GetCurrentMagazine().ReloadMagType.GetInt(),
                    AmmoInChamber = __instance.Weapon.ChamberAmmoCount,
                    SlotModeID = __instance.Weapon.GetMagazineSlot().FullId,
                    InMisfireMalfunction = __instance.Malfunction,
                    Boltcatch = __instance.Weapon.IsBoltCatch
                });

                coopPlayer.AddCommand(new GClass2144()
                {
                    Items = Components,
                    SlotModeID = __instance.Weapon.GetMagazineSlot().FullId,
                    MagTypeCurrent = __instance.Weapon.GetCurrentMagazine().ReloadMagType.GetInt(),
                    MagTypeNew = magazine.ReloadMagType.GetInt(),
                    AmmoInChamberCurrent = __instance.Weapon.ChamberAmmoCount,
                    ShellsInWeapon = __instance.Weapon.ShellsInChamberCount,
                    WeaponItemID = __instance.Weapon.Id,
                    WeaponLevel = 1,
                    InMisfireMalfunction = __instance.Malfunction,
                    Boltcatch = __instance.Weapon.IsBoltCatch,
                    NeedToAddAmmoInChamber = __instance.Weapon.CanLoadAmmoToChamber,
                    AmmoInMag = __instance.Weapon.GetCurrentMagazineCount(),
                    ForceEmptyChamber = false,
                    AmmoInChamberResult = 1
                });
            }
            else
            {
                Logger.LogError("No CoopPlayer found!");
            }
        }



        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }

    }
}
