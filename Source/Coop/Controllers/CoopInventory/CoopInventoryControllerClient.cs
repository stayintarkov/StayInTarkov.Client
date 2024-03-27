using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using static UnityEngine.UIElements.StyleVariableResolver;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public sealed class CoopInventoryControllerClient
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController, IIdGenerator
    {
        ManualLogSource BepInLogger { get; set; }

        //public MongoID? ForcedExpectedId { get; set; }
        public override MongoID NextId 
        { 
            get 
            {
                //BepInLogger.LogDebug("Getting NextId");
                //if (ForcedExpectedId != null) 
                //{
                //    var result = new MongoID(ForcedExpectedId.Value);
                //    ForcedExpectedId = null;
                //    BepInLogger.LogDebug($">> {result}");
                //    return result;
                //}

                mongoID_0++;
                BepInLogger.LogDebug($">> {mongoID_0}");
                return mongoID_0;
            } 
        }

        public CoopInventoryControllerClient(EFT.Player player, Profile profile, bool examined, string initialMongoId)
            : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryControllerClient));
            BepInLogger.LogDebug(nameof(CoopInventoryControllerClient));
            mongoID_0 = new MongoID(initialMongoId);
            mongoID_0--;
        }

        public string GetMongoId()
        {
            return mongoID_0;
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
            operation.vmethod_0((r) => 
            { 
                if (r.Failed) 
                    BepInLogger.LogError(r.Error); 

                if (r.Succeed)
                {
                    //BepInLogger.LogDebug(operation.ToJson());
                }

            });
        }


    }
}
