using EFT.InventoryLogic;
using System.Collections.Generic;

namespace StayInTarkov.Coop
{
    internal static class ItemAddressHelpers
    {
        private static string DICTNAMES_SlotItemAddressDescriptor { get; } = "sitad";
        private static string DICTNAMES_GridItemAddressDescriptor { get; } = "grad";

        public static void ConvertItemAddressToDescriptor(ItemAddress location, ref Dictionary<string, object> dictionary)
        {
            if (location is GridItemAddress gridItemAddress)
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = new()
                {
                    Container = new ContainerDescriptor
                    {
                        ContainerId = location.Container.ID,
                        ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null
                    },
                    LocationInGrid = gridItemAddress.LocationInGrid
                };
                dictionary.Add(DICTNAMES_GridItemAddressDescriptor, gridItemAddressDescriptor);
            }
            else if (location is SlotItemAddress slotItemAddress)
            {
                SlotItemAddressDescriptor slotItemAddressDescriptor = new()
                {
                    Container = new ContainerDescriptor
                    {
                        ContainerId = location.Container.ID,
                        ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null
                    }
                };

                dictionary.Add(DICTNAMES_SlotItemAddressDescriptor, slotItemAddressDescriptor);
            }
        }

        public static void ConvertDictionaryToAddress(
            Dictionary<string, object> dict,
            out GridItemAddressDescriptor gridItemAddressDescriptor,
            out SlotItemAddressDescriptor slotItemAddressDescriptor
            )
        {
            gridItemAddressDescriptor = null;
            slotItemAddressDescriptor = null;
            if (dict.ContainsKey(DICTNAMES_GridItemAddressDescriptor))
            {
                gridItemAddressDescriptor = StayInTarkovHelperConstants.SITParseJson<GridItemAddressDescriptor>(dict[DICTNAMES_GridItemAddressDescriptor].ToString());
            }

            if (dict.ContainsKey(DICTNAMES_SlotItemAddressDescriptor))
            {
                slotItemAddressDescriptor = StayInTarkovHelperConstants.SITParseJson<SlotItemAddressDescriptor>(dict[DICTNAMES_SlotItemAddressDescriptor].ToString());
            }
        }
    }
}
