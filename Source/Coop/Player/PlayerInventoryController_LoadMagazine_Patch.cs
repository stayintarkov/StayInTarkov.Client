using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Player
{
    internal class PlayerInventoryController_LoadMagazine_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_LoadMagazine";

        public static HashSet<string> CallLocally = new();

        public static HashSet<string> AlreadySent = new();

        public ManualLogSource GetLogger()
        {
            return GetLogger(typeof(PlayerInventoryController_LoadMagazine_Patch));
        }

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "LoadMagazine", false, true);
            return method;
        }

        public override void Enable()
        {
            base.Enable();

            AlreadySent.Clear();
        }

        [PatchPrefix]
        public static bool PrePatch(
            object __instance
            , ref Task<IResult> __result
            , BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions
            , Profile ___profile_0
            )
        {
            //Logger.LogInfo("PlayerInventoryController_LoadMagazine_Patch:PrePatch");
            var result = false;

            if (CallLocally.Contains(___profile_0.ProfileId))
                result = true;

            //__result = new Task<IResult>(() => { return null; });
            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            ItemController __instance
            , BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions
            , Profile ___profile_0)
        {
            //Logger.LogInfo("PlayerInventoryController_LoadMagazine_Patch:PostPatch");

            if (CallLocally.Contains(___profile_0.ProfileId))
            {
                CallLocally.Remove(___profile_0.ProfileId);
                return;
            }

            LoadMagazinePacket itemPacket = new(___profile_0.ProfileId, sourceAmmo.Id, sourceAmmo.TemplateId, magazine.Id, magazine.TemplateId
                , loadCount > 0 ? loadCount : sourceAmmo.StackObjectsCount
                , ignoreRestrictions);



            var serialized = itemPacket.Serialize();
            //Logger.LogInfo(serialized);

            //if(AlreadySent.Contains(serialized))
            //    return;

            //AlreadySent.Add(serialized);
            GameClient.SendDataToServer(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //taskScheduler.Do((s) =>
            //{
            //Logger.LogInfo($"PlayerInventoryController_LoadMagazine_Patch.Replicated");

            LoadMagazinePacket itemPacket = new(null, null, null, null, null, 0, false);

            if (dict.ContainsKey("data"))
            {
                itemPacket.Deserialize((byte[])dict["data"]);
            }
            else
            {
                return;
            }

            if (HasProcessed(GetType(), player, itemPacket))
                return;

            //if (CallLocally.ContainsKey(player.Profile.ProfileId))
            //    return;

            ////Logger.LogInfo($"ItemUiContext_ThrowItem_Patch.Replicated Profile Id {itemPacket.ProfileId}");

            if (!ItemFinder.TryFindItemController(player.ProfileId, out var invController))
            {
                GetLogger().LogError($"Replicated. Unable to find Player Item Controller");
                return;
            }

            if (ItemFinder.TryFindItem(itemPacket.SourceAmmoId, out Item bullet))
            {
                if (ItemFinder.TryFindItem(itemPacket.MagazineId, out Item magazine))
                {
                    CallLocally.Add(player.ProfileId);
                    //Logger.LogInfo($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Calling LoadMagazine ({bullet.Id}:{magazine.Id}:{itemPacket.LoadCount})");
                    invController.LoadMagazine((BulletClass)bullet, (MagazineClass)magazine, itemPacket.LoadCount);
                }
                else
                {
                    GetLogger().LogError($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.MagazineId}");
                }
            }
            else
            {
                GetLogger().LogError($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.SourceAmmoId}");
            }

            //});

        }

        public class LoadMagazinePacket : BasePlayerPacket
        {
            public string SourceAmmoId { get; set; }
            public string SourceTemplateId { get; set; }

            public string MagazineId { get; set; }
            public string MagazineTemplateId { get; set; }

            public int LoadCount { get; set; }

            public bool IgnoreRestrictions { get; set; }

            public LoadMagazinePacket(
                string profileId
                , string sourceAmmoId
                , string sourceTemplateId
                , string magazineId
                , string magazineTemplateId
                , int loadCount
                , bool ignoreRestrictions)
                : base(profileId, "PlayerInventoryController_LoadMagazine")
            {
                this.SourceAmmoId = sourceAmmoId;
                this.SourceTemplateId = sourceTemplateId;
                this.MagazineId = magazineId;
                this.MagazineTemplateId = magazineTemplateId;
                this.LoadCount = loadCount;
                this.IgnoreRestrictions = ignoreRestrictions;
            }
        }


    }
}
