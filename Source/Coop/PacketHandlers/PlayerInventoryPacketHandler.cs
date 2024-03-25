using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SIT.Core.Coop.PacketHandlers
{
    internal class PlayerInventoryPacketHandler : IPlayerPacketHandler
    {
        private SITGameComponent CoopGameComponent { get { SITGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;

        private static BepInEx.Logging.ManualLogSource Logger { get; set; }

        private HashSet<string> _processedPackets { get; } = new HashSet<string>();

        public PlayerInventoryPacketHandler()
        {
            //Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerInventoryPacketHandler));
        }

        static PlayerInventoryPacketHandler()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerInventoryPacketHandler));
        }

        public void ProcessPacket(Dictionary<string, object> packet)
        {
            string profileId = null;

            if (!packet.ContainsKey("profileId"))
                return;

            profileId = packet["profileId"].ToString();

            if (!packet.ContainsKey("m"))
                return;



            var packetJson = packet.ToJson();
            if (_processedPackets.Contains(packetJson))
                return;

            _processedPackets.Add(packetJson);

            //Logger.LogInfo(packet.ToJson());

            //ProcessPolymorphOperation(ref packet);
            //ProcessUnloadMagazine(ref packet);
            //ProcessMoveOperation(ref packet);
            //ProcessThrowOperation(ref packet);
            //ProcessFoldOperation(ref packet);
            //ProcessSearchOperation(ref packet);

        }

        public void ProcessPacket(byte[] packet)
        {
            throw new NotImplementedException();
        }

        //private void ProcessPolymorphOperation(ref Dictionary<string, object> packet)
        //{
        //    //if (packet["m"].ToString() != "PolymorphInventoryOperation")
        //    //    return;

        //    //var plyr = Players[packet["profileId"].ToString()];
        //    //var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
        //    //if (pic == null)
        //    //{
        //    //    Logger.LogError("Player Inventory Controller is null");
        //    //    return;
        //    //}

        //    //if (!packet.ContainsKey("data"))
        //    //    return;

        //    //var data = (byte[])packet["data"];
        //    //ProcessPolymorphOperation(plyr, data);
        //}

        //public void ProcessPolymorphOperation(EFT.Player plyr, byte[] data)
        //{

        //    if (plyr == null) return;

        //    if (data == null) return;   

        //    ItemPlayerPacket itemPlayerPacket = new ItemPlayerPacket(plyr.ProfileId, "", "", "PolymorphInventoryOperation");
        //    itemPlayerPacket.Deserialize(data);

        //    ProcessPolymorphOperation(plyr, itemPlayerPacket);
        //}

        public static void ProcessPolymorphOperation(EFT.Player plyr, ItemPlayerPacket itemPlayerPacket)
        {
            Logger.LogInfo("ProcessPolymorphOperation");
            if (plyr == null)
            {
                Logger.LogError("Player is null");
                return;
            }

            if (itemPlayerPacket == null)
            {
                Logger.LogError("itemPlayerPacket is null");
                return;
            }

            var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
            if (pic == null)
            {
                Logger.LogError("Player Inventory Controller is null");
                return;
            }

            if (itemPlayerPacket.OperationBytes == null)
            {
                Logger.LogError("packet has no OperationBytes");
                pic.CancelExecute(itemPlayerPacket.CallbackId);
                return;
            }

            AbstractDescriptor1 descriptor = null;
            using (MemoryStream memoryStream = new MemoryStream(itemPlayerPacket.OperationBytes))
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
            Logger.LogDebug($"{descriptor}");

            var operationResult = plyr.ToInventoryOperation(descriptor);

            Logger.LogDebug($"{operationResult}");
            Logger.LogDebug($"{operationResult.Value}");
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
                pic.CancelExecute(descriptor.OperationId);
            }
        }


    

        private void ProcessUnloadMagazine(ref Dictionary<string, object> packet)
        {
            //if (packet["m"].ToString() != "UnloadMagazine")
            //    return;

            //var plyr = Players[packet["profileId"].ToString()];
            //var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
            //if (pic == null)
            //{
            //    Logger.LogError("Player Inventory Controller is null");
            //    return;
            //}

            //if (!packet.ContainsKey("data"))
            //    return;

            //var data = (byte[])packet["data"];

            //ItemPlayerPacket itemPlayerPacket = new ItemPlayerPacket(plyr.ProfileId, "", "", "UnloadMagazine");
            //itemPlayerPacket = (ItemPlayerPacket)itemPlayerPacket.Deserialize(data);

            //if (ItemFinder.TryFindItem(itemPlayerPacket.ItemId, out var item)) 
            //{
            //    if (item is MagazineClass magazine)
            //        pic.ReceiveUnloadMagazine(magazine);
            //}
            //else
            //{
            //    Logger.LogError($"Couldnt find item {itemPlayerPacket.ItemId}");

            //    Logger.LogError(packet.ToJson());
            //}
        }

        //private void ProcessFoldOperation(ref Dictionary<string, object> packet)
        //{
        //    if (packet["m"].ToString() != "MoveOperation")
        //        return;

        //    if (!packet.ContainsKey("DynamicOperationType")
        //        || packet["DynamicOperationType"] == null
        //        || packet["DynamicOperationType"].ToString() != "FoldOperation")
        //        return;


        //    var packetJson = packet.ToJson();

        //    var plyr = Players[packet["profileId"].ToString()];
        //    MoveOperationPacket moveOperationPacket = JsonConvert.DeserializeObject<MoveOperationPacket>(packetJson);
        //    Logger.LogInfo(moveOperationPacket.ToJson());

        //    var moveOpDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(moveOperationPacket.MoveOpJson);
        //    var moveOpJO = JObject.Parse(moveOperationPacket.MoveOpJson);
        //    var parentId = moveOpJO["From"]["Container"]["ParentId"].ToString();
        //    Logger.LogInfo(parentId);

        //    FoldOperationDescriptor foldOperationDescriptor = JsonConvert.DeserializeObject<FoldOperationDescriptor>(JsonConvert.SerializeObject(moveOpDict));


        //    var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
        //    if (pic == null)
        //    {
        //        Logger.LogError("Player Inventory Controller is null");
        //        return;
        //    }

        //    if (ItemFinder.TryFindItem(parentId, out var item))
        //    {
        //        if (!ItemMovementHandler.CanFold(item, out var foldableComponent))
        //        {
        //            Logger.LogError("Attempted to fold an unfoldable component?");
        //            pic.CancelExecute(packetJson);
        //        }
        //        else
        //        {
        //            var foldOperation = new FoldOperation(foldOperationDescriptor.OperationId, pic, foldableComponent, true);
        //            pic.ReceiveExecute(foldOperation, packetJson);
        //        }
        //    }
        //}

        //private void ProcessMoveOperation(ref Dictionary<string, object> packet)
        //{
        //    if (packet["m"].ToString() != "MoveOperation")
        //        return;

        //    if (packet.ContainsKey("DynamicOperationType") && packet["DynamicOperationType"] != null)
        //        return;

        //    var packetJson = packet.ToJson();

        //    //Logger.LogInfo(packetJson);

        //    var plyr = Players[packet["profileId"].ToString()];

        //    //MoveOperationPacket moveOperationPacket = new(packet["profileId"].ToString(), null, null);
        //    //moveOperationPacket.DeserializePacketSIT(packet["data"].ToString());
        //    MoveOperationPacket moveOperationPacket = JsonConvert.DeserializeObject<MoveOperationPacket>(packetJson);

        //    Logger.LogInfo(moveOperationPacket.ToJson());
        //    //Logger.LogInfo(moveOperationPacket.MoveOpJson);
        //    var moveOpDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(moveOperationPacket.MoveOpJson);
        //    //if (!moveOpDict.ContainsKey("From"))
        //    //{
        //    //    Logger.LogError("No From Key found in MoveOperation");
        //    //    return;
        //    //}
        //    if (!moveOpDict.ContainsKey("To"))
        //    {
        //        Logger.LogError("No To Key found in MoveOperation");
        //        return;
        //    }
        //    AbstractDescriptor fromAD = null;

        //    if (packet.ContainsKey("FromAddressType") && moveOpDict.ContainsKey("From"))
        //    {
        //        switch (packet["FromAddressType"].ToString())
        //        {
        //            case "SlotItemAddress":
        //                fromAD = JsonConvert.DeserializeObject<SlotItemAddressDescriptor>(moveOpDict["From"].ToString());
        //                break;
        //            case "StackSlotItemAddress":
        //                fromAD = JsonConvert.DeserializeObject<StackSlotItemAddressDescriptor>(moveOpDict["From"].ToString());
        //                break;
        //            case "GridItemAddress":
        //                fromAD = JsonConvert.DeserializeObject<GridItemAddressDescriptor>(moveOpDict["From"].ToString());
        //                break;
        //            default:
        //                Logger.LogError($"Unknown FromAddressType {packet["FromAddressType"].ToString()}");
        //                break;
        //        }
        //    }

        //    //Logger.LogInfo(packet["ToAddressType"].ToString());
        //    AbstractDescriptor toAD = null;
        //    switch (packet["ToAddressType"].ToString())
        //    {
        //        case "SlotItemAddress":
        //            toAD = JsonConvert.DeserializeObject<SlotItemAddressDescriptor>(moveOpDict["To"].ToString());
        //            break;
        //        case "StackSlotItemAddress":
        //            toAD = JsonConvert.DeserializeObject<StackSlotItemAddressDescriptor>(moveOpDict["To"].ToString());
        //            break;
        //        case "GridItemAddress":
        //            toAD = JsonConvert.DeserializeObject<GridItemAddressDescriptor>(moveOpDict["To"].ToString());
        //            break;
        //        default:
        //            Logger.LogError($"Unknown ToAddressType {packet["ToAddressType"].ToString()}");
        //            break;
        //    }
        //    moveOpDict.Remove("From");
        //    moveOpDict.Remove("To");
        //    MoveOperationDescriptor moveOpDesc = JsonConvert.DeserializeObject<MoveOperationDescriptor>(JsonConvert.SerializeObject(moveOpDict));
        //    moveOpDesc.From = fromAD;
        //    moveOpDesc.To = toAD;

        //    var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
        //    if (pic == null)
        //    {
        //        Logger.LogError("Player Inventory Controller is null");
        //        return;
        //    }

        //    if (ItemFinder.TryFindItem(moveOpDesc.ItemId, out var item))
        //    {
        //        // This is a bad way to handle the error that the item doesn't exist on PlayerInventoryController
        //        try
        //        {
        //            MoveInternalOperation moveOperation = new MoveInternalOperation(moveOpDesc.OperationId, pic, item, pic.ToItemAddress(moveOpDesc.To), new List<ItemsCount>());
        //            pic.ReceiveExecute(moveOperation, packetJson);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogError($"{packetJson}");
        //            Logger.LogError($"{ex}");

        //            TraderControllerClass itemController = null;
        //            if (ItemFinder.TryFindItemController(moveOpDesc.To.Container.ParentId, out itemController))
        //            {
        //                Logger.LogError("TraderControllerClass not found! Falling back to To Container");
        //                MoveInternalOperation moveOperation = new MoveInternalOperation(moveOpDesc.OperationId, itemController, item, itemController.ToItemAddress(moveOpDesc.To), new List<ItemsCount>());
        //                pic.ReceiveExecute(moveOperation, packetJson);
        //            }
        //            else if (ItemFinder.TryFindItemController(moveOpDesc.From.Container.ParentId, out itemController))
        //            {
        //                Logger.LogError("TraderControllerClass not found! Falling back to From Container");
        //                MoveInternalOperation moveOperation = new MoveInternalOperation(moveOpDesc.OperationId, itemController, item, itemController.ToItemAddress(moveOpDesc.To), new List<ItemsCount>());
        //                pic.ReceiveExecute(moveOperation, packetJson);
        //            }
        //            else
        //            {
        //                Logger.LogError("TraderControllerClass not found!");
        //            }
        //        }
        //    }
        //}

        //private void ProcessThrowOperation(ref Dictionary<string, object> packet)
        //{
        //    if (packet["m"].ToString() != "ThrowOperation")
        //        return;

        //    var packetJson = packet.ToJson();

        //    var plyr = Players[packet["profileId"].ToString()];
        //    MoveOperationPacket moveOperationPacket = JsonConvert.DeserializeObject<MoveOperationPacket>(packetJson);

        //    var moveOpDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(moveOperationPacket.MoveOpJson);
        //    ThrowOperationDescriptor throwOpDesc = JsonConvert.DeserializeObject<ThrowOperationDescriptor>(JsonConvert.SerializeObject(moveOpDict));

        //    var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
        //    if (pic == null)
        //    {
        //        Logger.LogError("Player Inventory Controller is null");
        //        return;
        //    }

        //    if (ItemFinder.TryFindItem(throwOpDesc.ItemId, out var item))
        //    {
        //        pic.ReceiveExecute((MoveInternalOperation2)plyr.ToThrowOperation(throwOpDesc).Value, packetJson);
        //    }
        //}

        //private void ProcessSearchOperation(ref Dictionary<string, object> packet)
        //{
        //    if (packet["m"].ToString() != "SearchOperation")
        //        return;

        //    var packetJson = packet.ToJson();

        //    var plyr = Players[packet["profileId"].ToString()];
        //    MoveOperationPacket moveOperationPacket = JsonConvert.DeserializeObject<MoveOperationPacket>(packetJson);

        //    var moveOpDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(moveOperationPacket.MoveOpJson);
        //    ThrowOperationDescriptor throwOpDesc = JsonConvert.DeserializeObject<ThrowOperationDescriptor>(JsonConvert.SerializeObject(moveOpDict));

        //    var pic = ItemFinder.GetPlayerInventoryController(plyr) as CoopInventoryController;
        //    if (pic == null)
        //    {
        //        Logger.LogError("Player Inventory Controller is null");
        //        return;
        //    }

        //    if (ItemFinder.TryFindItem(throwOpDesc.ItemId, out var item))
        //    {
        //        pic.ReceiveExecute((MoveInternalOperation2)plyr.ToThrowOperation(throwOpDesc).Value, packetJson);
        //    }
        //}
    }
}
