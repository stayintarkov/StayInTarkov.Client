using BepInEx.Logging;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using JetBrains.Annotations;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Coop.Players;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public class CoopInventoryController
        // At this point in time. PlayerOwnerInventoryController is required to fix Malfunction and Discard errors. This class needs to be replaced with PlayerInventoryController.
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public HashSet<string> AlreadySent = new();

        private EFT.Player Player { get; set; }

        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base (player, profile, examined) 
        { 
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
            Player = player;
        }

        private Dictionary<ushort, (AbstractInventoryOperation operation, Callback callback)> InventoryOperations { get; } = new();

        public override void Execute(Operation1 operation, Callback callback)
        {
            // Debug the operation
            BepInLogger.LogDebug($"Execute(Operation1 operation,:{operation}");

            base.Execute(operation, callback);
        }

        protected readonly Dictionary<uint, Callback<int, bool, EOperationStatus>> OperationCallbacks = new Dictionary<uint, Callback<int, bool, EOperationStatus>>();

        private HashSet<ushort> IgnoreOperations = new();

        public uint CreateOperation<T>(T operation, Callback<int, bool, EOperationStatus> action) where T : AbstractInventoryOperation
        {
            ushort id = operation.Id;
            OperationCallbacks.Add(id, action);
            return id;
        }


        public override void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
            if (callback == null)
            {
                callback = delegate
                {
                };
            }
            EOperationStatus? localOperationStatus = null;
            if (!vmethod_0(operation))
            {
                operation.Dispose();
                callback.Fail("LOCAL: hands controller can't perform this operation");
                return;
            }
            //int finishedInventoryHash = 0;
            //EOperationStatus? serverOperationStatus;
            uint callbackId = CreateOperation(operation, delegate (Result<int, bool, EOperationStatus> result)
            {
                BepInLogger.LogInfo(string.Format("{0} {1} {2}", base.ID, operation.Id, operation));
                BepInLogger.LogInfo(string.Format("{0}", result));
                if (result.Succeed)
                {
                    operation.vmethod_0((er) => {
                    
                     callback.Succeed();

                    });
                    operation.Dispose();
                    //switch (result.Value2)
                    //{
                    //    case EOperationStatus.Finished:
                    //        finishedInventoryHash = result.Value0;
                    //        serverOperationStatus = EOperationStatus.Finished;
                    //        if (localOperationStatus == serverOperationStatus)
                    //        {
                    //            operation.Dispose();
                    //            callback.Succeed();
                    //        }
                    //        break;
                    //    case EOperationStatus.Started:
                    //        localOperationStatus = EOperationStatus.Started;
                    //        serverOperationStatus = EOperationStatus.Started;
                    //        operation.vmethod_0(delegate (IResult executeResult)
                    //        {
                    //            if (!executeResult.Succeed)
                    //            {
                    //                BepInLogger.LogError(string.Format("{0} - Client operation critical failure: {1} - {2}\r\nError: {3}", base.ID, operation.Id, operation, executeResult.Error));
                    //            }
                    //            localOperationStatus = EOperationStatus.Finished;
                    //            //if (serverOperationStatus == localOperationStatus)
                    //            {
                    //                operation.Dispose();
                    //                callback.Invoke(result);
                    //            }
                    //            //else if (serverOperationStatus.HasValue && serverOperationStatus == EOperationStatus.Failed)
                    //            //{
                    //            //    operation.Dispose();
                    //            //    callback.Invoke(result);
                    //            //}
                    //        }, requiresExternalFinalization: true);
                    //        break;
                    //}
                }
                else
                {
                    BepInLogger.LogError(string.Format("{0} - Client operation rejected by server: {1} - {2}\r\nReason: {3}", base.ID, operation.Id, operation, result.Error));
                    //serverOperationStatus = EOperationStatus.Failed;
                    EOperationStatus? eOperationStatus = localOperationStatus;
                    EOperationStatus eOperationStatus2 = EOperationStatus.Started;
                    if (!((eOperationStatus.GetValueOrDefault() == EOperationStatus.Started) & eOperationStatus.HasValue))
                    {
                        operation.Dispose();
                        callback.Invoke(result);
                    }
                }
            });
            SendExecuteOperationToServer(operation);
        }


        /// <summary>
        /// Create the packet to send to the Server
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        protected virtual void SendExecuteOperationToServer(AbstractInventoryOperation operation)
        {
            byte[] opBytes = null;
            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                var desc = OperationToDescriptorHelpers.FromInventoryOperation(operation, toObserver: false);
                BepInLogger.LogDebug($"Writing. {desc}");

                binaryWriter.WritePolymorph(desc);
                opBytes = memoryStream.ToArray();
            }

#if DEBUG
            AbstractDescriptor1 descriptor = null;
            using (MemoryStream msTestOutput = new MemoryStream(opBytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(msTestOutput))
                {
                    descriptor = binaryReader.ReadPolymorph<AbstractDescriptor1>();
                    BepInLogger.LogDebug($"Reading test. {descriptor}");
                }
            }
#endif

            var itemId = "";
            var templateId = "";
            PolymorphInventoryOperationPacket itemPlayerPacket = new PolymorphInventoryOperationPacket(Player.ProfileId, itemId, templateId);
            itemPlayerPacket.OperationBytes = opBytes;
            itemPlayerPacket.CallbackId = operation.Id;
            itemPlayerPacket.InventoryId = this.ID;

            BepInLogger.LogDebug($"Operation: {operation.GetType().Name}, IC Name: {this.Name}, {Player.name}");


            //ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", CommandStatus.Begin);

            GameClient.SendData(itemPlayerPacket.Serialize());


        }

        public void ReceiveExecute(AbstractInventoryOperation operation)
        {
            BepInLogger.LogInfo("ReceiveExecute");
            if (OperationCallbacks.TryGetValue(operation.Id, out var value))
            {
                //if (status != 0)
                //{
                //    OperationCallbacks.Remove(operation.Id);
                //}
                BepInLogger.LogInfo(">> OperationCallbacks");
                BepInLogger.LogInfo(value);

                value(new Result<int, bool, EOperationStatus>(0, false, EOperationStatus.Started));
                //value(new Result<int, bool, EOperationStatus>(0, false, EOperationStatus.Finished));
            }
            else
            {
                operation.vmethod_0((r) => { });
            }
            ////BepInLogger.LogInfo($"ReceiveExecute");
            ////BepInLogger.LogInfo($"{packetJson}");

            //if (operation == null)
            //    return;

            //if (IgnoreOperations.Contains(operation.Id))
            //{
            //    BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Ignoring:{operation}");
            //    return;
            //}

            ////BepInLogger.LogDebug($"ReceiveExecute:{operation}");

            //var callback = new Comfort.Common.Callback((result) => {
            //    if(result.Failed)
            //        BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:{result.Error}");
            //});
            //// Taken from ClientPlayer.Execute
            //if (!vmethod_0(operation))
            //{
            //    operation.Dispose();
            //    callback.Fail("LOCAL: hands controller can't perform this operation");
            //    return;
            //}

            //var cachedOperation = InventoryOperations.ContainsKey(operation.Id) ? InventoryOperations[operation.Id].operation : null;
            //// Operation created via this player
            //if (cachedOperation != null)
            //{
            //    cachedOperation.vmethod_0((executeResult) =>
            //    {
            //        var cachedOperationCallback = InventoryOperations[cachedOperation.Id].callback;

            //        //BepInLogger.LogInfo($"operation.vmethod_0 : {executeResult}");
            //        if (executeResult.Succeed)
            //        {
            //            //InventoryOperations[cachedOperation.Id].callback?.Succeed();

            //            RaiseInvEvents(cachedOperation, CommandStatus.Succeed);
            //            RaiseInvEvents(operation, CommandStatus.Succeed);
            //        }
            //        else
            //        {
            //            RaiseInvEvents(cachedOperation, CommandStatus.Failed);
            //            RaiseInvEvents(operation, CommandStatus.Failed);

            //            //InventoryOperations[cachedOperation.Id].callback?.Fail("Fail");
            //        }
            //        cachedOperation.Dispose();

            //    }, false);
            //}
            //else
            //{
            //    var ammoManipOp = operation as AbstractAmmoManipulationOperation;
            //    if (ammoManipOp != null)
            //    {
            //        //BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Is Ammo Manipulation");
            //        if (ammoManipOp.InternalOperation != null)
            //        {
            //            _cachedInternalOperation = ammoManipOp.InternalOperation;
            //            ammoManipOp.vmethod_0(callback, false);
            //        }
            //        else
            //        {
            //            BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Ammo Manipulation has no InternalOperation?");
            //            if (_cachedInternalOperation != null)
            //                _cachedInternalOperation.vmethod_0(callback, false);
            //        }
            //        return;
            //    }

            //    // Operation created by another player
            //    base.Execute(operation, callback);
            //}
        }

        AbstractInventoryOperation _cachedInternalOperation;

        void RaiseInvEvents(object operation, CommandStatus status)
        {
            if (operation == null)
                return;

            ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", status);
        }

        public void CancelExecute(ushort id)
        {
            BepInLogger.LogError($"CancelExecute");
            BepInLogger.LogError($"OperationId:{id}");
            // If operation created via this player, then cancel that operation
            var cachedOperation = InventoryOperations.ContainsKey(id) ? InventoryOperations[id].operation : null;
            if (cachedOperation != null)
            {
                cachedOperation.vmethod_0(delegate (IResult result)
                {
                    ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(cachedOperation, "commandStatus_0", CommandStatus.Failed);
                });
            }
        }

        public override async Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            //BepInLogger.LogDebug("LoadMagazine");
            //BepInLogger.LogDebug($"{sourceAmmo}:{magazine}:{loadCount}:{ignoreRestrictions}");
            //StopProcesses();
            ////return await base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
            //return await base.LoadMagazine(sourceAmmo, magazine, loadCount, true);

            NotificationManagerClass.DisplayWarningNotification("SIT: Unsupported feature [LoadMagazine]");

            return null;
        }

        public override async Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            //BepInLogger.LogDebug($"Starting UnloadMagazine for magazine {magazine.Id}");

            // --------------- TODO / FIXME ---------------------------------------------
            // KNOWN BUG
            // Paulov: This is 100% a workaround.
            // The vanilla call to unload each bullet individually causes an issue on other clients
            // Problem occurs because the bullets are not provided the ItemId that the client knows about so they cannot syncronize properly when it reached them
            //return await UnloadAmmoInstantly(magazine);

            //return await base.UnloadMagazine(magazine);

            NotificationManagerClass.DisplayWarningNotification("SIT: Unsupported feature [UnloadMagazine]");

            return null;

            //    // --------------------------------------------------------------------------
            //    // HELP / UNDERSTANDING
            //    // Use DNSpy/ILSpy to understand more on this process. EFT.Player.PlayerInventoryController.UnloadMagazine
            //    // The current process uses an Interfaced class to Start and asyncronously run unloading 1 bullet at a time based on skills etc
            //    //float magSkillSpeed = 100f - (float)Profile.Skills.MagDrillsUnloadSpeed + magazine.LoadUnloadModifier;
            //    //float unloadTime = Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime * magSkillSpeed / 100f;
            //    //var hasBulletsRemaining = true;


            //    //GOperationResult5 operationResult;
            //    //IPopNewAmmoResult unloadLocation = null;
            //    //Item firstBullet = null;
            //    //while (hasBulletsRemaining)
            //    //{
            //    //    BulletClass currentBullet = (BulletClass)magazine.Cartridges.Items.LastOrDefault();
            //    //    if (currentBullet == null)
            //    //        break;

            //    //    if (firstBullet != null && currentBullet.TemplateId != firstBullet.TemplateId)
            //    //        firstBullet = null;

            //    //    //var sourceOption = ItemMovementHandler.QuickFindAppropriatePlace(currentBullet, this, this.Inventory.Equipment.ToEnumerable(), ItemMovementHandler.EMoveItemOrder.UnloadAmmo, simulate: true);
            //    //    if (firstBullet == null || unloadLocation == null)
            //    //    {
            //    //        var result = ItemMovementHandler.QuickFindAppropriatePlace(currentBullet, this, this.Inventory.Equipment.ToEnumerable(), ItemMovementHandler.EMoveItemOrder.UnloadAmmo, false);
            //    //        if (result.Failed)
            //    //            break;

            //    //        if (firstBullet == null) 
            //    //        {
            //    //            firstBullet = result.Value.ResultItem;
            //    //        }
            //    //    }

            //    //    await Task.Delay(Mathf.CeilToInt(unloadTime * 1000f));

            //    //    //BepInLogger.LogDebug($"{nameof(sourceOption)}:{sourceOption}");

            //    //    //if (sourceOption.Failed)
            //    //    //    return sourceOption.ToResult();

            //    //    //ItemAddress itemAddress = null;
            //    //    //Item item = null;
            //    //    //IPopNewAmmoResult value = sourceOption.Value;

            //    //    //BepInLogger.LogDebug($"{nameof(value)}:{value}");

            //    //    //if (value != null)
            //    //    //{
            //    //    //    if (!(value is GIPopNewAmmoResult1 gIPopNewAmmoResult))
            //    //    //    {
            //    //    //        if (value is GIPopNewAmmoResult2 gIPopNewAmmoResult2)
            //    //    //        {
            //    //    //            item = gIPopNewAmmoResult2.TargetItem;
            //    //    //        }
            //    //    //    }
            //    //    //    else
            //    //    //    {
            //    //    //        itemAddress = gIPopNewAmmoResult.To;
            //    //    //        item = currentBullet;
            //    //    //    }
            //    //    //}

            //    //    //BepInLogger.LogDebug($"{nameof(itemAddress)}:{itemAddress}");
            //    //    //BepInLogger.LogDebug($"{nameof(item)}:{item}");

            //    //    //if (itemAddress == null && item == null)
            //    //    //    break;

            //    //    //await Task.Delay(Mathf.CeilToInt(unloadTime * 1000f));
            //    //    //global::SOperationResult12<GIOperationResult> sourceOption2 = ((firstBullet != null) ? BulletClass.ApplyToAmmo(item, firstBullet, 1, this, simulate: true) : BulletClass.ApplyToAddress(item, itemAddress, 1, this, simulate: true));

            //    //    //BepInLogger.LogDebug($"{nameof(sourceOption2)}:{sourceOption2.Value}");

            //    //    //if (sourceOption2.Failed)
            //    //    //{
            //    //    //    BepInLogger.LogError($"{nameof(sourceOption2)}:{sourceOption2.Error}");
            //    //    //    return sourceOption2.ToResult();
            //    //    //}
            //    //    //var operationResult = new GOperationResult5(sourceOption2.Value);
            //    //    //if (!operationResult.CanExecute(this))
            //    //    //    break;

            //    //    //var operation = this.ConvertOperationResultToOperation(operationResult);
            //    //    //this.ReceiveExecute(operation);
            //    //    //if (firstBullet == null)
            //    //    //    firstBullet = item;

            //    //    if (Singleton<GUISounds>.Instantiated)
            //    //        Singleton<GUISounds>.Instance.PlayUIUnloadSound();


            //    //    hasBulletsRemaining = magazine.Cartridges.Items.Count() > 0;
            //    //    //this.Execute(operation, delegate (IResult executeResult)
            //    //    //{
            //    //    //    executionSource.SetResult(executeResult);
            //    //    //});
            //    //    //IResult result = await executionSource.Task;
            //    //    //if (!result.Failed)
            //    //    //{
            //    //    //    if (firstBullet == null)
            //    //    //        firstBullet = item;
            //    //    //    //int_1--;
            //    //    //    //item.RaiseRefreshEvent();
            //    //    //    //magazine.RaiseRefreshEvent();
            //    //    //    //item_1?.RaiseRefreshEvent();

            //    //    //    if (Singleton<GUISounds>.Instantiated)
            //    //    //        Singleton<GUISounds>.Instance.PlayUIUnloadSound();
            //    //    //    continue;
            //    //    //}
            //    //    //return result;
            //    //}

            //    //return null;
        }

        //public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket)
        //{
        //    BepInLogger.LogInfo("ReceiveUnloadMagazineFromServer");
        //    if (ItemFinder.TryFindItem(unloadMagazinePacket.ItemId, out Item magazine))
        //    {
        //        base.UnloadMagazine((MagazineClass)magazine);

        //    }
        //}

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }

        public override void AddDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
            //base.AddDiscardLimits(rootItem, destroyedItems);
        }

        public override void SubtractFromDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
            //base.SubtractFromDiscardLimits(rootItem, destroyedItems);
        }

        public static bool IsDiscardLimitsFine(Dictionary<string, int> DiscardLimits)
        {
            return DiscardLimits != null
                && DiscardLimits.Count > 0
                && DiscardLimits.ContainsKey("5449016a4bdc2d6f028b456f") // Roubles, Value: 20000
                && DiscardLimits.ContainsKey("5696686a4bdc2da3298b456a") // Dollars, Value: 0
                && DiscardLimits.ContainsKey("569668774bdc2da2298b4568") // Euros, Value: 0
                && DiscardLimits.ContainsKey("5448be9a4bdc2dfd2f8b456a") // RGD-5 Grenade, Value: 20
                && DiscardLimits.ContainsKey("5710c24ad2720bc3458b45a3") // F-1 Grenade, Value: 20
                && DiscardLimits.ContainsKey(DogtagComponent.BearDogtagsTemplate) // Value: 0
                && DiscardLimits.ContainsKey(DogtagComponent.UsecDogtagsTemplate); // Value: 0
        }

        // PlayerOwnerInventoryController methods. We should inherit EFT.Player.PlayerInventoryController and override these methods based on EFT.Player.PlayerOwnerInventoryController

        public override void CallMalfunctionRepaired(Weapon weapon)
        {
            base.CallMalfunctionRepaired(weapon);
            if (!Player.IsAI && (bool)Singleton<SettingsManager>.Instance.Game.Settings.MalfunctionVisability)
            {
                MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.GlowType.Repaired, force: true, GetMalfunctionGlowAlphaMultiplier());
            }
        }

        private float GetMalfunctionGlowAlphaMultiplier()
        {
            float result = 0.5f;
            //if (Player.HealthController.FindActiveEffect<IEffect21>() != null)
            //{
            //    result = 1f;
            //}
            return result;
        }
    }


}
