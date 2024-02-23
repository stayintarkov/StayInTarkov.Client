using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using JetBrains.Annotations;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public class CoopInventoryController
        // At this point in time. PlayerOwnerInventoryController is required to fix Malfunction and Discard errors. This class needs to be replaced with PlayerInventoryController.
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public HashSet<string> AlreadySent = new();

        private EFT.Player Player { get; set; }

        private Dictionary<ushort, (AbstractInventoryOperation operation, Callback callback)> InventoryOperations { get; } = new();

        public override void Execute(Operation1 operation, Callback callback)
        {
            base.Execute(operation, callback);

            // Debug the operation
            BepInLogger.LogDebug($"Execute(Operation1 operation,:{operation}");
        }

        public override async void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
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

            // Debug the operation
            BepInLogger.LogDebug($"{operation}");
            // Set the operation to "Begin" (flashing)
            RaiseInvEvents(operation, CommandStatus.Begin);

            // Create the packet to send to the Server
            await Task.Delay(50);
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
                //operation.vmethod_0(delegate (IResult result)
                //{
                //    if (!result.Succeed)
                //    {
                //        Logger.LogError("[{0}][{5}] {1} - Local operation failed: {2} - {3}\r\nError: {4}", Time.frameCount, ID, operation.Id, operation, result.Error, Name);
                //    }
                //    callback?.Invoke(result);
                //});
            }
        }

        /// <summary>
        /// Create the packet to send to the Server
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private void SendExecuteOperationToServer(AbstractInventoryOperation operation)
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

            //BepInLogger.LogDebug($"ReceiveExecute:{operation}");

            var callback = new Comfort.Common.Callback((result) => {
                BepInLogger.LogError($"{nameof(ReceiveExecute)}:{result}"); 
                if(result.Failed)
                    BepInLogger.LogError($"{nameof(ReceiveExecute)}:{result.Error}");
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
            base.OutProcess(executor, item, from, to, operation, callback);
        }

        public override void InProcess(TraderControllerClass executor, Item item, ItemAddress to, bool succeed, IOperation1 operation, Callback callback)
        {
            // Taken from EFT.Player.PlayerInventoryController
            if (!succeed)
            {
                callback.Succeed();
                return;
            }
            base.InProcess(executor, item, to, succeed, operation, callback);
        }

        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
            Player = player;
            /*if (player.Side != EPlayerSide.Savage && !IsDiscardLimitsFine(DiscardLimits))
                ResetDiscardLimits();*/
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            BepInLogger.LogDebug("LoadMagazine");
            BepInLogger.LogDebug($"{sourceAmmo}:{magazine}:{loadCount}:{ignoreRestrictions}");
            return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
        }

        public override async Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            BepInLogger.LogDebug($"Starting UnloadMagazine for magazine {magazine.Id}");

            // --------------- TODO / FIXME ---------------------------------------------
            // KNOWN BUG
            // Paulov: This is 100% a workaround.
            // The vanilla call to unload each bullet individually causes an issue on other clients
            // Problem occurs because the bullets are not provided the ItemId that the client knows about so they cannot syncronize properly when it reached them
            return await UnloadAmmoInstantly(magazine);

            // --------------------------------------------------------------------------
            // HELP / UNDERSTANDING
            // Use DNSpy/ILSpy to understand more on this process. EFT.Player.PlayerInventoryController.UnloadMagazine
            // The current process uses an Interfaced class to Start and asyncronously run unloading 1 bullet at a time based on skills etc



            //int retryCount = 3;
            //int delayBetweenRetries = 500;

            //while (retryCount-- > 0)
            //{
            //    try
            //    {
            //        IResult result = await base.UnloadMagazine(magazine);
            //        if (result.Failed)
            //        {
            //            BepInLogger.LogError($"Failed to unload magazine {magazine.Id}: {result.Error}");
            //            if (retryCount > 0) await Task.Delay(delayBetweenRetries);
            //            else return result;
            //        }
            //        else
            //        {
            //            BepInLogger.LogDebug($"Successfully unloaded magazine {magazine.Id}");
            //            return SuccessfulResult.New;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        BepInLogger.LogError($"Exception in UnloadMagazine for magazine {magazine.Id}: {ex.Message}");
            //        if (retryCount <= 0) return new FailedResult($"Exception occurred: {ex.Message}", -1);
            //        await Task.Delay(delayBetweenRetries);
            //    }
            //}
            //return new FailedResult("Failed to unload magazine after multiple attempts.", -1);
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
            if (Player.HealthController.FindActiveEffect<IEffect21>() != null)
            {
                result = 1f;
            }
            return result;
        }
    }


}
