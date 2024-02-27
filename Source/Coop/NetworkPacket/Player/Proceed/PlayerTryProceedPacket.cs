//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
//{
//    public sealed class PlayerTryProceedPacket : PlayerProceedPacket
//    {
//        public PlayerTryProceedPacket() { }

//        public PlayerTryProceedPacket(string profileId, string itemId, string templateId, bool scheduled)
//           : base(profileId, itemId, templateId, scheduled, nameof(PlayerTryProceedPacket))
//        {
//        }

//        public override ISITPacket Deserialize(byte[] bytes)
//        {
//            return base.Deserialize(bytes);
//        }

//        public override byte[] Serialize()
//        {
//            return base.Serialize();
//        }
//    }
//}
