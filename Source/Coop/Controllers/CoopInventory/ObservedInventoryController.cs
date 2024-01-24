using BepInEx.Logging;
using EFT;
using StayInTarkov.Coop.NetworkPacket;
using System;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public class ObservedInventoryController
        // At this point in time. PlayerOwnerInventoryController is required to fix Malfunction and Discard errors. This class needs to be replaced with PlayerInventoryController.
        : EFT.Player.PlayerOwnerInventoryController, ICoopInventoryController
    {
        public ObservedInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
            Player = player;
        }

        public ManualLogSource BepInLogger { get; }
        public EFT.Player Player { get; }

        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket)
        {
            throw new NotImplementedException();
        }
    }
}
