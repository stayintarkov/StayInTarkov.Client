using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace StayInTarkov.Coop.PacketHandlers
{
    /// <summary>
    /// Created by Paulov
    /// </summary>
    internal class PlayerRotatePacketHandler : IPlayerPacketHandler
    {
        private CoopGameComponent CoopGameComponent { get { CoopGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;

        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        public PlayerRotatePacketHandler()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerRotatePacketHandler));
        }

        public void ProcessPacket(Dictionary<string, object> packet)
        {
            ProcessPacket(JObject.FromObject(packet));
        }

        public void ProcessPacket(JObject packet)
        {
            string profileId = null;
            string method = null;

            if (!packet.ContainsKey("profileId"))
                return;

            profileId = packet["profileId"].ToString();

            if (!packet.ContainsKey(AkiBackendCommunication.PACKET_TAG_METHOD))
                return;

            method = packet[AkiBackendCommunication.PACKET_TAG_METHOD].ToString();

            // Now Process dependant on the Packet
            // e.g. if (packet["m"].ToString() == "MoveOperation")
            if (method != "PlayerRotate")
                return;

            var player = (Players[profileId]);
            if (!player.GetComponent<PlayerReplicatedComponent>().IsClientDrone)
                return;
            //Logger.LogInfo(packet.SITToJson());

            var x = float.Parse(packet["x"].ToString());
            var y = float.Parse(packet["y"].ToString());

            //Logger.LogInfo(x);
            //Logger.LogInfo(y);
            Vector2 rot = new Vector2(x, y);
            player.ReceiveRotate(rot, false);
        }

        public void ProcessPacket(byte[] packet)
        {
        }
    }
}