using EFT.InventoryLogic;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Inventory
{
    internal class PlayerInventoryStopProcessesPacket : BasePlayerPacket
    {
        public PlayerInventoryStopProcessesPacket() : base("", nameof(PlayerInventoryStopProcessesPacket))
        { 
        }

        public PlayerInventoryStopProcessesPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerInventoryStopProcessesPacket))
        {
        }

        protected override void Process(CoopPlayerClient client)
        {
            ((CoopInventoryControllerClient)ItemFinder.GetPlayerInventoryController(client)).ReceiveStopProcesses();
        }
    }
}
