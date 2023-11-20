using Comfort.Common;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Spawners
{
    public class ItemFactory
    {
        private static object instance;

        private static MethodBase methodCreateItem;

        private static Dictionary<string, ItemTemplate> dict = new();

        public static void Init()
        {
            Type type = StayInTarkovHelperConstants.EftTypes.Single((Type x) => x.GetMethod("LogErrors") != null);
            Type type2 = typeof(Singleton<>).MakeGenericType(type);
            instance = type2.GetProperty("Instance").GetValue(type2);
            methodCreateItem = type.GetMethod("CreateItem");
            dict = (Dictionary<string, ItemTemplate>)type.GetField("ItemTemplates").GetValue(instance);
        }

        public static Item CreateItem(string id, string tpid)
        {
            if (methodCreateItem == null)
            {
                Init();
            }
            return (Item)methodCreateItem.Invoke(instance, new object[3] { id, tpid, null });
        }

        public static ItemTemplate GetItemTemplateById(string id)
        {
            if (methodCreateItem == null)
            {
                Init();
            }
            return dict[id];
        }
    }

}
