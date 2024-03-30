//using EFT;
//using EFT.InventoryLogic;
//using StayInTarkov.Coop.Components.CoopGameComponents;
//using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
//using StayInTarkov.Coop.Players;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityStandardAssets.Water;

//namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
//{
//    public sealed class TriggerPressedPacket : BasePlayerPacket
//    {
//        public bool Pressed { get; set; }
//        public float RotationX { get; set; }
//        public float RotationY { get; set; }

//        public TriggerPressedPacket() { }

//        public TriggerPressedPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(TriggerPressedPacket))
//        {
//        }

//        public override byte[] Serialize()
//        {
//            var ms = new MemoryStream();
//            using BinaryWriter writer = new BinaryWriter(ms);
//            WriteHeaderAndProfileId(writer);
//            writer.Write(Pressed);
//            writer.Write(RotationX);
//            writer.Write(RotationY);
//            writer.Write(TimeSerializedBetter);

//            return ms.ToArray();
//        }

//        public override ISITPacket Deserialize(byte[] bytes)
//        {
//            if (bytes == null)
//                throw new ArgumentNullException(nameof(bytes));

//            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
//            ReadHeaderAndProfileId(reader);
//            Pressed = reader.ReadBoolean();
//            RotationX = reader.ReadSingle();
//            RotationY = reader.ReadSingle();
//            TimeSerializedBetter = reader.ReadString();

//            return this;
//        }

//        protected override void Process(CoopPlayerClient client)
//        {
//            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
//            {
//                coopGameComponent.UpdatePing(GetTimeSinceSent().Milliseconds);
//                if (client.HandsController is EFT.Player.FirearmController fc)
//                {
//                    fc.SetTriggerPressed(Pressed);
//                }
//            }
//        }

//    }
//}
