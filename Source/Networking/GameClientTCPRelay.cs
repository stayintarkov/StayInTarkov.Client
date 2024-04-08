using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Networking
{
    public class GameClientTCPRelay : MonoBehaviour, IGameClient
    {
        public BlockingCollection<byte[]> PooledBytesToPost { get; } = new BlockingCollection<byte[]>();

        void Awake()
        {
            AkiBackendCommunication.Instance.WebSocketClose();
            AkiBackendCommunication.Instance.WebSocketCreate(SITMatchmaking.Profile);
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

        public void SendData(byte[] data)
        {
            if (data == null)
                return;

            PooledBytesToPost.Add(data);
        }

        public void SendData<T>(ref T packet) where T : BasePacket
        {
            SendData(packet.Serialize());
        }
    }
}
