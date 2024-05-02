using StayInTarkov.Coop.Players;

namespace StayInTarkov.Coop.NetworkPacket.Player.Inventory
{
    /// <summary>
    /// Goal: Backpack dropping in Polymorphing an inventory operation is too slow and only happens after the animation has already played
    /// This causes an issue because the host still has to sync up by the time the client might have already picked up a new bag.
    /// This Packet and the code referencing it fixes that.
    /// </summary>
    internal class PlayerInventoryDropBackpackPacket : ItemPlayerPacket
    {
        public PlayerInventoryDropBackpackPacket() : base("", "", "", nameof(PlayerInventoryDropBackpackPacket))
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
            client.DropBackpack();
        }
    }
}
