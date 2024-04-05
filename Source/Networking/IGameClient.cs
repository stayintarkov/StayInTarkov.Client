using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public interface IGameClient
    {
        /// <summary>
        /// Latency to server in milliseconds (RTT / 2)
        /// </summary>
        public ushort Ping { get; }
        public float UploadSpeedKbps { get; }
        public float DownloadSpeedKbps { get; }
        public uint PacketLoss { get; }

        /// <summary>
        /// Reset stats and compute download/upload speed and packet loss
        /// </summary>
        public void ResetStats();

        /// <summary>
        /// Will send bytes to the Server
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data);

        /// <summary>
        /// Will send bytes to the Server by Serializing the Packet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        public void SendData<T>(ref T packet) where T : BasePacket;


    }
}
