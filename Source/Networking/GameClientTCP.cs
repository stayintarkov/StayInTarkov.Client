using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public class GameClientTCP : GameClient
    {
        public GameClientTCP(BepInEx.Configuration.ConfigFile config) 
        { 
        
        }

        public override void SendDataToServer(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
