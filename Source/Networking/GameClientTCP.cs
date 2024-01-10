using StayInTarkov.Coop.Matchmaker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class GameClientTCP : MonoBehaviour, IGameClient
    {
        public BlockingCollection<byte[]> PooledBytesToPost { get; } = new BlockingCollection<byte[]>();

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

            if (PooledBytesToPost != null)
            {
                while (PooledBytesToPost.Any())
                {
                    while (PooledBytesToPost.TryTake(out var bytes))
                    {
                        AkiBackendCommunication.Instance.PostDownWebSocketImmediately(bytes);
                    }
                }
            }
        }

        public void SendDataToServer(byte[] data)
        {
            if (data == null)
                return;

            PooledBytesToPost.Add(data);
        }

       
    }
}
