using EFT.InventoryLogic;
using SIT.Tarkov.Core;
using System.Collections.Generic;

namespace SIT.Core.Coop
{
    internal static class ItemAddressHelpers
    {
        private static string DICTNAMES_SlotItemAddressDescriptor { get; } = "sitad";
        private static string DICTNAMES_GridItemAddressDescriptor { get; } = "grad";

        public static void ConvertItemAddressToDescriptor(ItemAddress location, ref Dictionary<string, object> dictionary)
        {
            if (location is GridItemAddress gridItemAddress)
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = new();
                gridItemAddressDescriptor.Container = new ContainerDescriptor();
                gridItemAddressDescriptor.Container.ContainerId = location.Container.ID;
                gridItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;
                gridItemAddressDescriptor.LocationInGrid = gridItemAddress.LocationInGrid;
                dictionary.Add(DICTNAMES_GridItemAddressDescriptor, gridItemAddressDescriptor);
            }
            else if (location is SlotItemAddress slotItemAddress)
            {
                SlotItemAddressDescriptor slotItemAddressDescriptor = new();
                slotItemAddressDescriptor.Container = new ContainerDescriptor();
                slotItemAddressDescriptor.Container.ContainerId = location.Container.ID;
                slotItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;

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
