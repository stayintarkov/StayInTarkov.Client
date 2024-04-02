using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System.IO;
using System.Linq;
using System.Text;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    /// <summary>
    /// TODO: Properly handle this with objects and properties, not JSON!
    /// </summary>
    public sealed class PlayerInteractWithObjectPacket : BasePlayerPacket
    {

        public JObject ProcessJson { get; set; }

        public PlayerInteractWithObjectPacket()
        {
        }

        public PlayerInteractWithObjectPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerInteractWithObjectPacket))
        {
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.WriteLengthPrefixedBytes(Encoding.UTF8.GetBytes(ProcessJson.ToString()));
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            var jsonBytes = reader.ReadLengthPrefixedBytes();
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            ProcessJson = JObject.Parse(jsonString);   

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInteractWithObjectPacket)}:{nameof(Process)}");

            if (!ItemFinder.TryFindItemController(client.ProfileId, out TraderControllerClass itemController))
            {
                StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated:Could not find {nameof(itemController)}");
                return;
            }

            WorldInteractiveObject worldInteractiveObject = SITGameComponent.GetCoopGameComponent().ListOfInteractiveObjects.FirstOrDefault(x => x.Id == ProcessJson["WIOId"].ToString());

            if (worldInteractiveObject == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"Player_ExecuteDoorInteraction_Patch:Replicated:Could not find {nameof(worldInteractiveObject)}");
                return;
            }

            InteractionResult interactionResult = new((EInteractionType)int.Parse(ProcessJson["interactionType"].ToString()));
            KeyInteractionResult keyInteractionResult = null;

            if (ProcessJson.ContainsKey("keyItemId"))
            {
                string itemId = ProcessJson["keyItemId"].ToString();
                if (!ItemFinder.TryFindItem(itemId, out Item item))
                    item = Spawners.ItemFactory.CreateItem(itemId, ProcessJson["keyTemplateId"].ToString());

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
                        StayInTarkovHelperConstants.Logger.LogError($"Player_StartDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not a KeyComponent object.");
                    }
                }
                else
                {
                    StayInTarkovHelperConstants.Logger.LogError($"Player_StartDoorInteraction_Patch:Replicated. Packet contain KeyInteractionResult but item {itemId} is not found.");
                }
            }


            client.vmethod_0(worldInteractiveObject, keyInteractionResult ?? interactionResult, () => { });
        }


    }
}
