using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public class GameClientTCP : GameClient
    {
        public override void SendDataToServer(byte[] data)
        {
            base.SendDataToServer(data);
        }
    }
}
