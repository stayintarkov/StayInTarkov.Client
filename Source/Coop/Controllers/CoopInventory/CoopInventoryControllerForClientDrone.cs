using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.NetworkPacket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    internal sealed class CoopInventoryControllerForClientDrone
        // At this point in time. PlayerOwnerInventoryController is required to fix Malfunction and Discard errors. This class needs to be replaced with PlayerInventoryController.
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerForClientDrone(EFT.Player player, Profile profile, bool examined)
            : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryControllerForClientDrone));

            if (profile.ProfileId.StartsWith("pmc") && !CoopInventoryController.IsDiscardLimitsFine(DiscardLimits))
                ResetDiscardLimits();
        }

        //public override void Execute(SearchContentOperation operation, Callback callback)
        //{
        //    base.Execute(operation, callback);
        //}

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            //return base.UnloadMagazine(magazine);

            return SuccessfulResult.Task;
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
                base.UnloadMagazine((MagazineClass)magazine).ContinueWith(x =>
                {
                    ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(unloadMagazinePacket.ProfileId);
                });
            }
        }
    }
}
