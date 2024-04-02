using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public sealed class PlayerInteractWithDoorPacket : BasePlayerPacket
    {

        public JObject ProcessJson { get; set; }
        public string DoorId { get; internal set; }

        public PlayerInteractWithDoorPacket()
        {
        }

        public PlayerInteractWithDoorPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerInteractWithDoorPacket))
        {
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(DoorId);
            writer.WriteLengthPrefixedBytes(Encoding.UTF8.GetBytes(ProcessJson.ToString()));
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            DoorId = reader.ReadString();
            var jsonBytes = reader.ReadLengthPrefixedBytes();
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            ProcessJson = JObject.Parse(jsonString);

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInteractWithDoorPacket)}:{nameof(Process)}");

            InteractionResult interactionResult = new((EInteractionType)int.Parse(ProcessJson["interactionType"].ToString()));
            KeyInteractionResult keyInteractionResult = null;

            if (ProcessJson.ContainsKey("keyItemId"))
            {
                string itemId = ProcessJson["keyItemId"].ToString();
                if (!ItemFinder.TryFindItem(itemId, out Item item))
                    item = Spawners.ItemFactory.CreateItem(itemId, ProcessJson["keyTemplateId"].ToString());


                if (!ItemFinder.TryFindItemController(client.ProfileId, out TraderControllerClass itemController))
                {
                    StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated:Could not find {nameof(itemController)}");
                    return;
                }

                if (item != null)
                {
                    if (item.TryGetItemComponent(out KeyComponent keyComponent))
                    {
                        DiscardResult discardResult = null;

                        if (ProcessJson.ContainsKey("keyParentGrid"))
                        {
                            ItemAddress itemAddress = itemController.ToGridItemAddress(ProcessJson["keyParentGrid"].ToString().SITParseJson<GridItemAddressDescriptor>());
                            discardResult = new DiscardResult(new RemoveResult(item, itemAddress, itemController, new ResizeResult(item, itemAddress, ItemMovementHandler.ResizeAction.Addition, null, null), null, false), null, null, null);
                        }

                        keyInteractionResult = new KeyInteractionResult(keyComponent, discardResult, bool.Parse(ProcessJson["succeed"].ToString()));
                    }
                    else
                    {
                        StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not a KeyComponent object.");
                    }
                }
                else
                {
                    StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not found.");
                }
            }


            WorldInteractiveObject worldInteractiveObject = SITGameComponent.GetCoopGameComponent().ListOfInteractiveObjects.FirstOrDefault(x => x.Id == ProcessJson["WIOId"].ToString());
            if (worldInteractiveObject == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated:Could not find {nameof(worldInteractiveObject)}");
                return;
            }

            // Interact with the door
            client.vmethod_1(worldInteractiveObject, keyInteractionResult ?? interactionResult);
        }


    }
}
