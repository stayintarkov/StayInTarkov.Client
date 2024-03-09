using Aki.Custom.Airdrops.Models;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using StayInTarkov.Networking;
using System.Linq;
using UnityEngine;

namespace Aki.Custom.Airdrops.Utils
{
    public class ItemFactoryUtil
    {
        private readonly ItemFactory itemFactory;

        public ItemFactoryUtil()
        {
            itemFactory = Singleton<ItemFactory>.Instance;
        }

        public void BuildContainer(LootableContainer container, AirdropConfigModel config, string dropType)
        {
            var containerId = config.ContainerIds[dropType];
            if (itemFactory.ItemTemplates.TryGetValue(containerId, out var template))
            {
                Item item = itemFactory.CreateItem(containerId, template._id, null);
                LootItem.CreateLootContainer(container, item, item.LocalizedName(), Singleton<GameWorld>.Instance);
            }
            else
            {
                Debug.LogError($"[AKI-AIRDROPS]: unable to find template: {containerId}");
            }
        }

        public async void AddLoot(LootableContainer container, AirdropLootResultModel lootToAdd)
        {
            Item actualItem;
            foreach (var item in lootToAdd.Loot)
            {
                ResourceKey[] resources;
                if (item.IsPreset)
                {
                    actualItem = itemFactory.GetPresetItem(item.Tpl);
                    actualItem.SpawnedInSession = true;
                    actualItem.GetAllItems().ExecuteForEach(x => x.SpawnedInSession = true);
                    resources = actualItem.GetAllItems().Select(x => x.Template).SelectMany(x => x.AllResources).ToArray();
                }
                else
                {
                    actualItem = itemFactory.CreateItem(item.ID, item.Tpl, null);
                    actualItem.StackObjectsCount = item.StackCount;
                    actualItem.SpawnedInSession = true;

                    resources = actualItem.Template.AllResources.ToArray();
                }

                container.ItemOwner.MainStorage[0].Add(actualItem);
                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.Immediate, null, PoolManager.DefaultCancellationToken);
            }
        }

        public AirdropLootResultModel GetLoot()
        {
            var json = AkiBackendCommunication.Instance.GetJson("/client/location/getAirdropLoot");
            var result = JsonConvert.DeserializeObject<AirdropLootResultModel>(json);

            return result;
        }
    }
}