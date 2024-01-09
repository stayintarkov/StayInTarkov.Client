using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{

    /// <summary>
    /// This is unused. TCP Mode is PRP (Peer->Relay->Peer via the Web Server)
    /// </summary>
    public class GameServerTCP : MonoBehaviour, IGameClient
    {
        public void SendDataToServer(byte[] data)
        {
            AkiBackendCommunication.Instance.SendDataToPool(data);
        }
    }
}
