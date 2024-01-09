using EFT;
using System;

namespace StayInTarkov.Coop.Controllers
{
    public class ObservedInventoryController(EFT.Player player, Profile profile, bool examined) : EFT.Player.PlayerInventoryController(player, profile, examined)
    {

    }
}
