using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SIT.Core.Coop.ItemControllerPatches;
using SIT.Core.Coop.NetworkPacket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    internal class CoopInventoryControllerForClientDrone 
        : InventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerForClientDrone(EFT.Player player, Profile profile, bool examined) 
            : base(profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
        }

        public override void Execute(SearchContentOperation operation, Callback callback)
        {
            base.Execute(operation, callback);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            //return base.UnloadMagazine(magazine);

            return SuccessfulResult.Task;
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            destroyedItems = new List<ItemsCount>();
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }

        public void ReceiveUnloadMagazineFromServer(UnloadMagazinePacket unloadMagazinePacket)
        {
            BepInLogger.LogInfo("ReceiveUnloadMagazineFromServer");
            if (ItemFinder.TryFindItem(unloadMagazinePacket.MagazineId, out Item magazine))
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
