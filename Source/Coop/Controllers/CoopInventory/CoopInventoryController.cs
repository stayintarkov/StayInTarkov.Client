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

        public string GetMongoId()
        {
            return mongoID_0;
        }


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
            var itemId = "";
            var templateId = "";
            ushort stackObjectsCount = 1;

            byte[] opBytes = null;
            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                var desc = OperationToDescriptorHelpers.FromInventoryOperation(operation, toObserver: false);
                //BepInLogger.LogDebug($"Writing. {desc}");
                var magOp = desc as MagOperationDescriptor;
                if (magOp != null)
                {
                    desc = magOp.InternalOperationDescriptor;
                }

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
            return await base.LoadMagazine(sourceAmmo, magazine, loadCount, true);

            //NotificationManagerClass.DisplayWarningNotification("SIT: Unsupported feature [LoadMagazine]");

            //return null;
        }

      

        public override async Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            //BepInLogger.LogDebug($"Starting UnloadMagazine for magazine {magazine.Id}");

            //PlayerInventoryUnloadMagazinePacket packet = new();
            //packet.ProfileId = Player.ProfileId;
            //packet.ItemId = magazine.Id;
            //GameClient.SendData(packet.Serialize());

            return await base.UnloadMagazine(magazine);

            //NotificationManagerClass.DisplayWarningNotification("SIT: Unsupported feature [UnloadMagazine]");

            //return null;
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

        public override bool CheckTransferOwners(Item item, ItemAddress targetAddress, out Error error)
        {
            error = null;
            return true;
        }
    }


}
