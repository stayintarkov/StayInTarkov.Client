using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using JetBrains.Annotations;
//using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private HashSet<AbstractInventoryOperation> InventoryOperations { get; } = new();

        public override void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback)
        {
            // If operation created via this player, then play out that operation
            if (InventoryOperations.Any(x => x.Id == operation.Id))
            {
                base.Execute(InventoryOperations.First(x => x.Id == operation.Id), callback);
                return;
            }
            //base.Execute(operation, callback);
            //RaiseInvEvents(operation, CommandStatus.Begin);

            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream))
            {
                var desc = OperationToDescriptorHelpers.FromInventoryOperation(operation, false, false);
                binaryWriter.WritePolymorph(desc);
                var opBytes = memoryStream.ToArray();

                var itemId = "";
                var templateId = "";
                if (operation is MoveInternalOperation moveOperation) 
                {
                    itemId = moveOperation.Item.Id;
                    templateId = moveOperation.Item.TemplateId;
                }
                if (operation is MoveInternalOperation1 otherOperation)
                {
                    itemId = otherOperation.Item.Id;
                    templateId = otherOperation.Item.TemplateId;
                }
                if (operation is MoveInternalOperation2 throwOperation)
                {
                    itemId = throwOperation.Item.Id;
                    templateId = throwOperation.Item.TemplateId;
                }

                ItemPlayerPacket itemPlayerPacket = new ItemPlayerPacket(Player.ProfileId, itemId, templateId, "PolymorphInventoryOperation");
                itemPlayerPacket.OperationBytes = opBytes;
                itemPlayerPacket.CallbackId = operation.Id;
                itemPlayerPacket.InventoryId = this.ID;

                BepInLogger.LogInfo($"Operation: {operation.GetType().Name}, IC Name: {this.Name}, {Player.name}");
                EFT.UI.ConsoleScreen.Log($"Operation: {operation.GetType().Name}, IC Name: {this.Name}, {Player.name}");

                BepInLogger.LogInfo(itemPlayerPacket);
                var s = itemPlayerPacket.Serialize();
                GameClient.SendDataToServer(s);
                InventoryOperations.Add(operation);
            }
        }

        public void ReceiveExecute(AbstractInventoryOperation operation, string packetJson)
        {
            BepInLogger.LogInfo($"ReceiveExecute");
            //BepInLogger.LogInfo($"{packetJson}");

            if (operation == null)
                return;

            BepInLogger.LogInfo($"{operation}");

            var cachedOperation = InventoryOperations.FirstOrDefault(x => x.Id == operation.Id);
            // Operation created via this player
            if (cachedOperation != null)
            {
                cachedOperation.vmethod_0((executeResult) =>
                {

                    BepInLogger.LogInfo($"operation.vmethod_0 : {executeResult}");
                    if (executeResult.Succeed)
                    {
                        ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(cachedOperation, "commandStatus_0", CommandStatus.Succeed);
                        RaiseInvEvents(cachedOperation, CommandStatus.Succeed);
                        cachedOperation.Dispose();
                    }
                    else
                    {
                        ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(cachedOperation, "commandStatus_0", CommandStatus.Failed);
                    }
                    cachedOperation.Dispose();


                }, false);
            }
            // Operation created by another player
            else
            {
                base.Execute(operation, (result) => { });
            }


            //base.Execute(operation, (IResult) => {

            //    RaiseInvEvents(operation, CommandStatus.Succeed);

            //});

            //ReceivedOperationPacket = operation;
            //ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", CommandStatus.Begin);
            //if (OperationCallbacks.ContainsKey(packetJson))
            //{
            //    BepInLogger.LogInfo($"Using OperationCallbacks!");

            //    //OperationCallbacks[packetJson].Item1.vmethod_0(delegate (IResult result)
            //    //{
            //    //    //ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(OperationCallbacks[packetJson].Item1, "commandStatus_0", CommandStatus.Succeed);
            //    //});
            //    RaiseInvEvents(operation, CommandStatus.Succeed);
            //    RaiseInvEvents(OperationCallbacks[packetJson].Item1, CommandStatus.Succeed);
            //    OperationCallbacks[packetJson].Item2();
            //    //OperationCallbacks[packetJson].Item1.vmethod_0((IResult result) => { RaiseInvEvents(operation, CommandStatus.Succeed); }, true);
            //    OperationCallbacks.Remove(packetJson);
            //}
            //else
            //{
            //    operation.vmethod_0(delegate (IResult result)
            //    {
            //        ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", CommandStatus.Succeed);
            //    });

            //}
        }

        void RaiseInvEvents(object operation, CommandStatus status)
        {
            if (operation == null)
                return;

            ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", status);
        }

        public void CancelExecute(uint id)
        {
            BepInLogger.LogError($"CancelExecute");
            BepInLogger.LogInfo($"{id}");
            // If operation created via this player, then cancel that operation
            var operation = InventoryOperations.FirstOrDefault(x => x.Id == id);
            if(operation != null)
            {
                operation.vmethod_0(delegate (IResult result)
                {
                    ReflectionHelpers.SetFieldOrPropertyFromInstance<CommandStatus>(operation, "commandStatus_0", CommandStatus.Succeed);
                });
            }
        }



        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
            Player = player;
            if (profile.ProfileId.StartsWith("pmc") && !IsDiscardLimitsFine(DiscardLimits))
                ResetDiscardLimits();
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            //BepInLogger.LogInfo("LoadMagazine");
            return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            Task<IResult> result;
            //ItemControllerHandler_Move_Patch.DisableForPlayer.Add(Profile.ProfileId);

            BepInLogger.LogInfo("UnloadMagazine");
            ItemPlayerPacket unloadMagazinePacket = new(Profile.ProfileId, magazine.Id, magazine.TemplateId, "PlayerInventoryController_UnloadMagazine");
            var serialized = unloadMagazinePacket.Serialize();

            //if (AlreadySent.Contains(serialized))
            {
                result = base.UnloadMagazine(magazine);
                //ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            }

            //AlreadySent.Add(serialized);

            GameClient.SendDataToServer(serialized);
            result = base.UnloadMagazine(magazine);
            //ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            return result;
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }

        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket)
        {
            BepInLogger.LogInfo("ReceiveUnloadMagazineFromServer");
            if (ItemFinder.TryFindItem(unloadMagazinePacket.ItemId, out Item magazine))
            {
                //ItemControllerHandler_Move_Patch.DisableForPlayer.Add(unloadMagazinePacket.ProfileId);
                base.UnloadMagazine((MagazineClass)magazine);
                //ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(unloadMagazinePacket.ProfileId);

            }
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
