using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static EFT.UI.CharacterSelectionStartScreen;
using static GClass2755;

namespace StayInTarkov.Coop.NetworkPacket.Player.Inventory
{
    /// <summary>
    /// A PolymorphInventoryOperationPacket is a packet that is sent when a character does anything with its inventory or loot
    /// See CoopInventoryController for more details
    /// </summary>
    public class PolymorphInventoryOperationPacket : ItemPlayerPacket
    {

        static PolymorphInventoryOperationPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PolymorphInventoryOperationPacket));
        }
        /// <summary>
        /// This is called via Reflection
        /// </summary>
        public PolymorphInventoryOperationPacket() : base("", "", "", nameof(PolymorphInventoryOperationPacket))
        {

        }

        /// <summary>
        /// This is called when creating the packet from the InventoryController
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="itemId"></param>
        /// <param name="templateId"></param>
        public PolymorphInventoryOperationPacket(string profileId, string itemId, string templateId) : base(new string(profileId.ToCharArray()), itemId, templateId, nameof(PolymorphInventoryOperationPacket))
        {

        }

        public static ManualLogSource Logger { get; }

        protected override void Process(CoopPlayerClient client)
        {

            if (this.OperationBytes == null)
            {
                Logger.LogError("packet has no OperationBytes");
                return;
            }

            ProcessOperationBytes(client, OperationBytes);

            
        }

        private void ProcessOperationBytes(CoopPlayerClient client, byte[] bytes)
        {
            var pic = ItemFinder.GetPlayerInventoryController(client) as CoopInventoryControllerClient;
            if (pic == null)
            {
                Logger.LogError("Player Inventory Controller is null");
                return;
            }

            AbstractDescriptor1 descriptor = null;
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    descriptor = binaryReader.ReadPolymorph<AbstractDescriptor1>();
                }
            }

            if (descriptor == null)
            {
                Logger.LogError("descriptor is null");
                return;
            }

            Logger.LogDebug($"{descriptor.GetType().Name}");

            if (descriptor is UnloadMagOperationDescriptor umod)
            {
                if (umod.InternalOperationDescriptor != null)
                {
                    Logger.LogDebug($"{umod.InternalOperationDescriptor.GetType().Name}");

                    using (MemoryStream ms = new MemoryStream())
                    using (var binaryWriter = new BinaryWriter(ms))
                    {
                        binaryWriter.WritePolymorph(umod.InternalOperationDescriptor);
                        ProcessOperationBytes(client, ms.ToArray()); 
                    }
                }
              
                return;
            }

            //if (descriptor is SplitOperationDescriptor split)
            //{
            //    Logger.LogDebug($"{descriptor.ToJson()}");

            //}

            //if (descriptor is LoadMagOperationDescriptor)
            //{
            //    Logger.LogDebug($"Not processing {nameof(LoadMagOperationDescriptor)}");
            //    return;
            //}


            // Paulov: This is a bit of a hack but it does work. 
            // If an item for some reason doesn't exist on this person. Create it with the same Id as provided.
            if (!string.IsNullOrEmpty(ItemId) && !string.IsNullOrEmpty(TemplateId))
            {
                if (!ItemFinder.TryFindItem(ItemId, out var item))
                {
                    Logger.LogDebug($"Item of Id {ItemId} was not found. Creating item! {TemplateId}");
                    Item knownItem = Spawners.ItemFactory.CreateItem(ItemId, TemplateId);
                    if (StackObjectsCount > 1)
                        knownItem.StackObjectsCount = StackObjectsCount;

                    if (descriptor is MoveOperationDescriptor moveOperationDescriptor)
                    {
                        var address = pic.ToItemAddress(moveOperationDescriptor.From);
                        ItemMovementHandler.Add(knownItem, address, pic);
                    }
                }
            }

            var operationResult = client.ToInventoryOperation(descriptor);
            
            //Logger.LogDebug($"{operationResult}");
            //Logger.LogDebug($"{operationResult.Value}");

            //if(operationResult.Value is GAbstractOperation126 splitOp)
            //{
            //    pic.ForcedExpectedId = splitOp.CloneId;
            //}


            if (operationResult.Succeeded)
            {
                pic.ReceiveExecute(operationResult.Value);
            }
            else
            {
                if (operationResult.Failed)
                {
                    StayInTarkovHelperConstants.Logger.LogError(operationResult.Error);
                }
            }
        }

    }
}
