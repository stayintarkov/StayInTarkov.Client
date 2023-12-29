using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public class GameClient : IGameClient
    {
        public virtual void SendDataToServer(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
