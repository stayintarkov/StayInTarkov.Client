using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayInTarkov.Coop
{
    internal class CoopInventoryController
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public HashSet<string> AlreadySent = new();


        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
        }

        public override void AddDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
        }

        public override void SubtractFromDiscardLimits(Item rootItem, IEnumerable<ItemsCount> destroyedItems)
        {
        }

        public override void Execute(SearchContentOperation operation, Callback callback)
        {
            //BepInLogger.LogInfo($"CoopInventoryController: {operation}");
            base.Execute(operation, callback);
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            //BepInLogger.LogInfo("LoadMagazine");
            return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            Task<IResult> result;
            ItemControllerHandler_Move_Patch.DisableForPlayer.Add(Profile.ProfileId);

            BepInLogger.LogInfo("UnloadMagazine");
            ItemPlayerPacket unloadMagazinePacket = new(Profile.ProfileId, magazine.Id, magazine.TemplateId, "PlayerInventoryController_UnloadMagazine");
            var serialized = unloadMagazinePacket.Serialize();

            //if (AlreadySent.Contains(serialized))
            {
                result = base.UnloadMagazine(magazine);
                ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            }

            //AlreadySent.Add(serialized);

            AkiBackendCommunication.Instance.SendDataToPool(serialized);
            result = base.UnloadMagazine(magazine);
            ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            return result;
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }

        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket)
        {
            BepInLogger.LogInfo("ReceiveUnloadMagazineFromServer");
            if (ItemFinder.TryFindItem(unloadMagazinePacket.ItemId, out Item magazine))
            {
                ItemControllerHandler_Move_Patch.DisableForPlayer.Add(unloadMagazinePacket.ProfileId);
                base.UnloadMagazine((MagazineClass)magazine);
                ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(unloadMagazinePacket.ProfileId);

            }
        }
    }


    public interface ICoopInventoryController
    {
        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket);
    }
}
