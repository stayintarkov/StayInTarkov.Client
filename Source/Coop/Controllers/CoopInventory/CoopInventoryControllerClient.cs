using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public sealed class CoopInventoryControllerClient
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerClient(EFT.Player player, Profile profile, bool examined)
            : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryControllerClient));
            BepInLogger.LogDebug(nameof(CoopInventoryControllerClient));
        }

        public override bool HasDiscardLimits => false;

        public override void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
            operation.vmethod_0((r) => { callback?.Succeed(); });
        }

        public override void Execute(Operation1 operation, Callback callback)
        {
            operation.vmethod_0((r) => { callback?.Succeed(); });
        }


        public override void CallUnknownMalfunctionStartRepair(Weapon weapon)
        {
            base.CallUnknownMalfunctionStartRepair(weapon);
        }

        public override void ExamineMalfunction(Weapon weapon, bool clearRest = false)
        {
            base.ExamineMalfunction(weapon, clearRest);
        }

        public override void CallMalfunctionRepaired(Weapon weapon)
        {
            base.CallMalfunctionRepaired(weapon);
        }

        public override void StrictCheckMagazine(MagazineClass magazine, bool status, int skill = 0, bool notify = false, bool useOperation = true)
        {
        }

        public override void OnAmmoLoadedCall(int count)
        {
        }

        public override void OnAmmoUnloadedCall(int count)
        {
        }

        public override void OnMagazineCheckCall()
        {
        }

        public override bool IsInventoryBlocked()
        {
            return false;
        }

        public override ToggleItem ToggleItem(TogglableComponent togglable)
        {
            var result = base.ToggleItem(togglable);
            return result;
        }

        public override void SubtractFromDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
            //base.SubtractFromDiscardLimits(rootItem, destroyedItems);
        }

        public override void AddDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
            //base.AddDiscardLimits(rootItem, destroyedItems);
        }

        public override void ExamineMalfunctionType(Weapon weapon)
        {
            //base.ExamineMalfunctionType(weapon);
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            //return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
            return null;
        }

        public override async Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            BepInLogger.LogDebug("UnloadMagazine");
            return await base.UnloadMagazine(magazine);
        }

        public override void StopProcesses()
        {
            //BepInLogger.LogDebug("StopProcesses");
            base.StopProcesses();
        }

        public void ReceiveStopProcesses()
        {
            BepInLogger.LogDebug("ReceiveStopProcesses");
            base.StopProcesses();
        }

        public override void ExecuteStop(Operation1 operation)
        {
            base.ExecuteStop(operation);
        }

        public void ReceiveExecute(AbstractInventoryOperation operation)
        {
            BepInLogger.LogDebug("ReceiveExecute");
            operation.vmethod_0((r) => { });
        }


    }
}
