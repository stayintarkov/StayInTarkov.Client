using BepInEx.Logging;
using Comfort.Common;
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
using System.Threading.Tasks;
using UnityEngine;
using UnloadAmmoOperation = GClass2863;

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

        private HashSet<ushort> IgnoreOperations = new();

        public override void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
            // Debug the operation
            BepInLogger.LogDebug($"{nameof(Execute)}{nameof(AbstractInventoryOperation)}");
            BepInLogger.LogDebug($"{operation}");

            // Paulov: Fix issue with assigning items to Quick Bar
            if(operation != null && operation.GetType() == typeof(GAbstractOperation15))
            {
                base.Execute(operation, callback);
                return;
            }

            // Paulov: Fix issue with unload magazines
            if (operation != null && operation.GetType() == typeof(UnloadAmmoOperation))
            {
                IgnoreOperations.Add(operation.Id);

                base.Execute(operation, callback);
                SendExecuteOperationToServer(operation);
                return;
            }

            // Taken from ClientPlayer.Execute
            if (callback == null)
            {
                callback = delegate
                {
                };
            }

            // Taken from ClientPlayer.Execute
            if (!vmethod_0(operation))
            {
                BepInLogger.LogError("LOCAL: hands controller can't perform this operation");
                BepInLogger.LogError(operation);
                operation.Dispose();
                callback.Fail("LOCAL: hands controller can't perform this operation");
                return;
            }
            // Check to see if the item is a Quest Item, if it is, do not send.
            if(operation is MoveInternalOperation moveInternalOperation)
            {
                if (moveInternalOperation.Item.QuestItem)
                {
                    base.Execute(operation, callback);
                    return;
                }
            }

            // If operation created via this player, then play out that operation
            if (InventoryOperations.ContainsKey(operation.Id))
            {
                base.Execute(InventoryOperations[operation.Id].operation, InventoryOperations[operation.Id].callback);
                return;
            }

         
            // Set the operation to "Begin" (flashing)
            RaiseInvEvents(operation, CommandStatus.Begin);

            // Create the packet to send to the Server
            SendExecuteOperationToServer(operation);
            InventoryOperations.Add(operation.Id, (operation, callback));


            if (!vmethod_0(operation))
            {
                operation.Dispose();
                callback?.Fail($"Can't execute {operation}", 1);
                return;
            }
            else
            {
                callback?.Succeed();
            }

            // Perform Inventory Sync
            if (this.Player != null && this.Player is CoopPlayer coopPlayer)
            {
                foreach (var item in this.Player.Inventory.AllRealPlayerItems.Where(x => x != null))
                {
                    float newValue = 0;
                    if (item is MedsClass meds)
                        newValue = meds.MedKitComponent != null ? meds.MedKitComponent.HpResource : 0;
                    else if (item is FoodClass food)
                        newValue = food.FoodDrinkComponent != null ? food.FoodDrinkComponent.HpPercent : 0;
                    PlayerPostProceedDataSyncPacket postProceedPacket = new PlayerPostProceedDataSyncPacket(coopPlayer.ProfileId, item.Id, newValue, item.StackObjectsCount);
                    GameClient.SendData(postProceedPacket.Serialize());
                }
            }
        }

        /// <summary>
        /// Create the packet to send to the Server
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        protected virtual void SendExecuteOperationToServer(AbstractInventoryOperation operation)
        {
            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                var ammoManipOp = operation as AbstractAmmoManipulationOperation;

                var desc = OperationToDescriptorHelpers.FromInventoryOperation(operation, false, false);
                //if (desc is UnloadMagOperationDescriptor unloadMagOpDesc)
                //{
                //    BepInLogger.LogDebug($"{nameof(SendExecuteOperationToServer)}:{nameof(unloadMagOpDesc)}:{unloadMagOpDesc}");
                //    if(unloadMagOpDesc.InternalOperationDescriptor == null)
                //    {
                //        BepInLogger.LogDebug($"{nameof(SendExecuteOperationToServer)}:{nameof(unloadMagOpDesc)}:{nameof(unloadMagOpDesc.InternalOperationDescriptor)} is null?");
                //    }
                //}
                binaryWriter.WritePolymorph(desc);
                var opBytes = memoryStream.ToArray();

                var itemId = "";
                var templateId = "";
                if (operation is MoveInternalOperation moveOperation)
                {
                    itemId = moveOperation.Item.Id;
                    templateId = moveOperation.Item.TemplateId;
                }
                //if (operation is MoveInternalOperation1 otherOperation)
                //{
                //    itemId = otherOperation.Item.Id;
                //    templateId = otherOperation.Item.TemplateId;
                //}
                //if (operation is MoveInternalOperation2 throwOperation)
                //{
                //    itemId = throwOperation.Item.Id;
                //    templateId = throwOperation.Item.TemplateId;
                //}

                PolymorphInventoryOperationPacket itemPlayerPacket = new PolymorphInventoryOperationPacket(Player.ProfileId, itemId, templateId);
                itemPlayerPacket.OperationBytes = opBytes;
                itemPlayerPacket.CallbackId = operation.Id;
                itemPlayerPacket.InventoryId = this.ID;

                BepInLogger.LogDebug($"Operation: {operation.GetType().Name}, IC Name: {this.Name}, {Player.name}");


                ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", CommandStatus.Begin);

                var s = itemPlayerPacket.Serialize();
                GameClient.SendData(s);
            }


        }

        public void ReceiveExecute(AbstractInventoryOperation operation)
        {
            //BepInLogger.LogInfo($"ReceiveExecute");
            //BepInLogger.LogInfo($"{packetJson}");

            if (operation == null)
                return;

            if (IgnoreOperations.Contains(operation.Id))
            {
                BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Ignoring:{operation}");
                return;
            }

            //BepInLogger.LogDebug($"ReceiveExecute:{operation}");

            var callback = new Comfort.Common.Callback((result) => {
                if(result.Failed)
                    BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:{result.Error}");
            });
            // Taken from ClientPlayer.Execute
            if (!vmethod_0(operation))
            {
                operation.Dispose();
                callback.Fail("LOCAL: hands controller can't perform this operation");
                return;
            }

            var cachedOperation = InventoryOperations.ContainsKey(operation.Id) ? InventoryOperations[operation.Id].operation : null;
            // Operation created via this player
            if (cachedOperation != null)
            {
                cachedOperation.vmethod_0((executeResult) =>
                {
                    var cachedOperationCallback = InventoryOperations[cachedOperation.Id].callback;

                    //BepInLogger.LogInfo($"operation.vmethod_0 : {executeResult}");
                    if (executeResult.Succeed)
                    {
                        //InventoryOperations[cachedOperation.Id].callback?.Succeed();

                        RaiseInvEvents(cachedOperation, CommandStatus.Succeed);
                        RaiseInvEvents(operation, CommandStatus.Succeed);
                    }
                    else
                    {
                        RaiseInvEvents(cachedOperation, CommandStatus.Failed);
                        RaiseInvEvents(operation, CommandStatus.Failed);

                        //InventoryOperations[cachedOperation.Id].callback?.Fail("Fail");
                    }
                    cachedOperation.Dispose();

                }, false);
            }
            else
            {
                var ammoManipOp = operation as AbstractAmmoManipulationOperation;
                if (ammoManipOp != null)
                {
                    //BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Is Ammo Manipulation");
                    if (ammoManipOp.InternalOperation != null)
                    {
                        _cachedInternalOperation = ammoManipOp.InternalOperation;
                        ammoManipOp.vmethod_0(callback, false);
                    }
                    else
                    {
                        BepInLogger.LogDebug($"{nameof(ReceiveExecute)}:Ammo Manipulation has no InternalOperation?");
                        if (_cachedInternalOperation != null)
                            _cachedInternalOperation.vmethod_0(callback, false);
                    }
                    return;
                }

                // Operation created by another player
                base.Execute(operation, callback);
            }
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

        public override void OutProcess(TraderControllerClass executor, Item item, ItemAddress from, ItemAddress to, IOperation1 operation, Callback callback)
        {
            //bool hasRootItemError = false;
            //try
            //{
            //    item.GetRootItem();
            //}
            //catch
            //{
            //    hasRootItemError = true;
            //}


            //if (!hasRootItemError)
                base.OutProcess(executor, item, from, to, operation, callback);
        }

        public override void InProcess(TraderControllerClass executor, Item item, ItemAddress to, bool succeed, IOperation1 operation, Callback callback)
        {
            // Taken from EFT.Player.PlayerInventoryController
            //if (!succeed)
            //{
            //    callback.Succeed();
            //    return;
            //}
            base.InProcess(executor, item, to, succeed, operation, callback);
        }

        public override async Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            BepInLogger.LogDebug("LoadMagazine");
            BepInLogger.LogDebug($"{sourceAmmo}:{magazine}:{loadCount}:{ignoreRestrictions}");
            StopProcesses();
            //return await base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
            return await base.LoadMagazine(sourceAmmo, magazine, loadCount, true);
        }

        public override async Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            BepInLogger.LogDebug($"Starting UnloadMagazine for magazine {magazine.Id}");
            StopProcesses();

            // --------------- TODO / FIXME ---------------------------------------------
            // KNOWN BUG
            // Paulov: This is 100% a workaround.
            // The vanilla call to unload each bullet individually causes an issue on other clients
            // Problem occurs because the bullets are not provided the ItemId that the client knows about so they cannot syncronize properly when it reached them
            //return await UnloadAmmoInstantly(magazine);

            return await base.UnloadMagazine(magazine);

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
