using EFT;
using EFT.Interactive;
using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GClass1885;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class LootableContainerInteractionPacket : BasePacket
    {
        public LootableContainerInteractionPacket() : base(nameof(LootableContainerInteractionPacket))
        {

        }

        public string LootableContainerId { get; set; }
        public EInteractionType InteractionType { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(LootableContainerId);
            writer.Write((byte)InteractionType);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            LootableContainerId = reader.ReadString();
            InteractionType = (EInteractionType)reader.ReadByte();
            return this;
        }

        public override void Process()
        {
            SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
            LootableContainer lootableContainer = coopGameComponent.ListOfInteractiveObjects.FirstOrDefault(x => x.Id == this.LootableContainerId) as LootableContainer;

            if (lootableContainer == null)
                return;

            string methodName = string.Empty;
            switch (InteractionType)
            {
                case EInteractionType.Open:
                    methodName = "Open";
                    break;
                case EInteractionType.Close:
                    methodName = "Close";
                    break;
                case EInteractionType.Unlock:
                    methodName = "Unlock";
                    break;
                case EInteractionType.Breach:
                    break;
                case EInteractionType.Lock:
                    methodName = "Lock";
                    break;
            }

            void Interact() => ReflectionHelpers.InvokeMethodForObject(lootableContainer, methodName);

            if (InteractionType == EInteractionType.Unlock)
                Interact();
            else
                lootableContainer.StartBehaviourTimer(EFTHardSettings.Instance.DelayToOpenContainer, Interact);

        }
    }
}
