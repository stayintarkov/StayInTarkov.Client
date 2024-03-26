using BepInEx.Logging;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using JetBrains.Annotations;
using StayInTarkov.Coop.NetworkPacket.Player.Inventory;
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
            operation.vmethod_0(delegate (IResult result)
            {
                if (!result.Succeed)
                {
                    BepInLogger.LogError(string.Format("[{0}][{5}] {1} - Local operation failed: {2} - {3}\r\nError: {4}", Time.frameCount, ID, operation.Id, operation, result.Error, Name));
                }
                callback?.Invoke(result);

                if (result.Succeed)
                    SendExecuteOperationToServer(operation);
            });

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
                //BepInLogger.LogDebug($"Writing. {desc}");

                binaryWriter.WritePolymorph(desc);
                opBytes = memoryStream.ToArray();
            }

//#if DEBUG
//            AbstractDescriptor1 descriptor = null;
//            using (MemoryStream msTestOutput = new MemoryStream(opBytes))
//            {
//                using (BinaryReader binaryReader = new BinaryReader(msTestOutput))
//                {
//                    descriptor = binaryReader.ReadPolymorph<AbstractDescriptor1>();
//                    BepInLogger.LogDebug($"Reading test. {descriptor}");
//                }
//            }
//#endif

            var itemId = "";
            var templateId = "";
            ushort stackObjectsCount = 1;
            if(operation is MoveInternalOperation mio)
            {
                itemId = mio.Item.Id;
                templateId = mio.Item.TemplateId;
                stackObjectsCount = (ushort)mio.Item.StackObjectsCount;
            }

            PolymorphInventoryOperationPacket itemPlayerPacket = new PolymorphInventoryOperationPacket(Player.ProfileId, itemId, templateId);
            itemPlayerPacket.OperationBytes = opBytes;
            itemPlayerPacket.CallbackId = operation.Id;
            itemPlayerPacket.InventoryId = this.ID;
            itemPlayerPacket.StackObjectsCount = stackObjectsCount;
            

            BepInLogger.LogDebug($"Operation: {operation.GetType().Name}, IC Name: {this.Name}, {Player.name}");

            GameClient.SendData(itemPlayerPacket.Serialize());


        }

        AbstractInventoryOperation _cachedInternalOperation;

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
            BepInLogger.LogDebug($"Starting UnloadMagazine for magazine {magazine.Id}");

            PlayerInventoryUnloadMagazinePacket packet = new();
            packet.ProfileId = Player.ProfileId;
            packet.ItemId = magazine.Id;
            GameClient.SendData(packet.Serialize());

            // --------------- TODO / FIXME ---------------------------------------------
            // KNOWN BUG
            // Paulov: This is 100% a workaround.
            // The vanilla call to unload each bullet individually causes an issue on other clients
            // Problem occurs because the bullets are not provided the ItemId that the client knows about so they cannot syncronize properly when it reached them
            //return await UnloadAmmoInstantly(magazine);

            return await base.UnloadMagazine(magazine);

            //NotificationManagerClass.DisplayWarningNotification("SIT: Unsupported feature [UnloadMagazine]");

            //return null;

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

        public override void StopProcesses()
        {
            BepInLogger.LogDebug($"StopProcesses");

            base.StopProcesses();

            PlayerInventoryStopProcessesPacket packet = new();
            packet.ProfileId = Player.ProfileId;
            GameClient.SendData(packet.Serialize());

        }

        public override void ExecuteStop(Operation1 operation)
        {
            BepInLogger.LogDebug($"ExecuteStop");

            base.ExecuteStop(operation);
        }


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
