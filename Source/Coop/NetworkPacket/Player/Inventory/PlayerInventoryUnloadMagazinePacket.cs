using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Inventory
{
    public sealed class PlayerInventoryUnloadMagazinePacket : ItemPlayerPacket
    {
        /// <summary>
        /// This is called via Reflection
        /// </summary>
        public PlayerInventoryUnloadMagazinePacket() : base("", "", "", nameof(PlayerInventoryUnloadMagazinePacket))
        {
        }

        public override byte[] Serialize()
        {
            return base.Serialize();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            return base.Deserialize(bytes);
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (ItemFinder.TryFindItem(ItemId, out var item) && item is MagazineClass magazine)
                _ = ((CoopInventoryControllerClient)ItemFinder.GetPlayerInventoryController(client)).UnloadMagazine(magazine);
        }
    }
}
