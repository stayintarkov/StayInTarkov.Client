using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;

//using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.NetworkPacket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public sealed class CoopInventoryControllerForClientDrone
        : CoopInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerForClientDrone(EFT.Player player, Profile profile, bool examined)
            : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryControllerForClientDrone));

            if(CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent)) 
            {
                if (coopGameComponent.PlayerUsers.Contains(player) && !CoopInventoryController.IsDiscardLimitsFine(DiscardLimits))
                    ResetDiscardLimits();
            }

        }

        public override void CallMalfunctionRepaired(Weapon weapon)
        {
            base.CallMalfunctionRepaired(weapon);
        }

    }
}
