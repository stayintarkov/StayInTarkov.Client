using Newtonsoft.Json;

namespace SIT.Core.Coop.NetworkPacket
{
    public class ItemPacket : BasePacket
    {
        [JsonProperty(PropertyName = "iid")]
        public string ItemId { get; set; }

        [JsonProperty(PropertyName = "tpl")]
        public string TemplateId { get; set; }

        public ItemPacket(string itemId, string templateId, string method)
        {
            ItemId = itemId;
            TemplateId = templateId;
            Method = method;
        }
    }

    public class ItemPlayerPacket : BasePlayerPacket
    {
        [JsonProperty(PropertyName = "iid")]
        public string ItemId { get; set; }

        [JsonProperty(PropertyName = "tpl")]
        public string TemplateId { get; set; }

        public ItemPlayerPacket(string profileId, string itemId, string templateId, string method)
            : base(profileId, method)
        {
            ItemId = itemId;
            TemplateId = templateId;
            Method = method;
        }
    }

    public class ItemMovePlayerPacket : ItemPlayerPacket
    {
        [JsonProperty(PropertyName = "sitad")]
        public string SlotItemAddressDescriptorJson { get; set; }

        [JsonProperty(PropertyName = "grad")]
        public string GridItemAddressDescriptorJson { get; set; }

        [JsonProperty(PropertyName = "ssad")]
        public string StackSlotItemAddressDescriptorJson { get; set; }

        public ItemMovePlayerPacket(string profileId, string itemId, string templateId, string method
            
            , SlotItemAddressDescriptor slotItemAddressDescriptor
            , GridItemAddressDescriptor gridItemAddressDescriptor
            , StackSlotItemAddressDescriptor stackSlotItemAddressDescriptor


            )
            : base(profileId, itemId, templateId, method)
        {
            if ( slotItemAddressDescriptor != null )
                SlotItemAddressDescriptorJson = slotItemAddressDescriptor.ToJson();

            if ( gridItemAddressDescriptor != null )
                GridItemAddressDescriptorJson = gridItemAddressDescriptor.ToJson();

            if( stackSlotItemAddressDescriptor != null )
                StackSlotItemAddressDescriptorJson = stackSlotItemAddressDescriptor.ToJson();

        }
    }
}
