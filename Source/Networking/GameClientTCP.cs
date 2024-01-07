using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class GameClientTCP : MonoBehaviour, IGameClient
    {
        public GameClientTCP(BepInEx.Configuration.ConfigFile config) 
        { 
        
        }
        //WebSocketSharp.WebSocket WebSocket { get; set; }

        void Awake()
        {
            AkiBackendCommunication.Instance.WebSocketClose();
            AkiBackendCommunication.Instance.WebSocketCreate(MatchmakerAcceptPatches.Profile);
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public void SendDataToServer(byte[] data)
        {
            AkiBackendCommunication.Instance.SendDataToPool(data);
        }

       
    }
}
