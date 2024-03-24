using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public sealed class PlayerProceedKnifePacket : ItemPlayerPacket
    {
        public bool Scheduled { get; set; }
        public bool QuickKnife { get; set; }

        public PlayerProceedKnifePacket() : base("", "", "", nameof(PlayerProceedKnifePacket))
        {

        }

        public PlayerProceedKnifePacket(string profileId, string itemId, bool scheduled, bool quickKnife)
            : base(profileId, itemId, "", nameof(PlayerProceedKnifePacket))
        {
            Scheduled = scheduled;
            QuickKnife = quickKnife;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(ItemId);
            writer.Write(Scheduled);
            writer.Write(QuickKnife);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            ItemId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            QuickKnife = reader.ReadBoolean();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                coopGameComponent.UpdatePing(GetTimeSinceSent().Milliseconds);

            if (ItemFinder.TryFindItem(ItemId, out var item))
            {
                if(QuickKnife)
                    client.Proceed(item.GetItemComponent<KnifeComponent>(), (Comfort.Common.Result<IQuickKnifeKickController> x) => { }, Scheduled);
                else
                    client.Proceed(item.GetItemComponent<KnifeComponent>(), (Comfort.Common.Result<IKnifeController> x) => { }, Scheduled);
            }
        }
    }
}
