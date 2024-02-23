using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public sealed class CoopInventoryControllerClient
        : CoopInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerClient(EFT.Player player, Profile profile, bool examined)
            : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryControllerClient));
        }

        public override bool HasDiscardLimits => false;

        public override void CallMalfunctionRepaired(Weapon weapon)
        {
            base.CallMalfunctionRepaired(weapon);
        }

        public override void StrictCheckMagazine(MagazineClass magazine, bool status, int skill = 0, bool notify = false, bool useOperation = true)
        {
        }

        public override void OnAmmoLoadedCall(int count)
        {
        }

        public override void OnAmmoUnloadedCall(int count)
        {
        }

        public override void OnMagazineCheckCall()
        {
        }

        public override bool IsInventoryBlocked()
        {
            return false;
        }

        public override void StartSearchingAction(SearchableItemClass item)
        {
            base.StartSearchingAction(item);
        }

        public override void StopSearchingAction(SearchableItemClass item)
        {
            base.StopSearchingAction(item);
        }
    }
}
