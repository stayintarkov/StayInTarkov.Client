using StayInTarkov.Coop.Components;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace StayInTarkov.Coop.PacketHandlers
{
    /// <summary>
    /// Created by Paulov
    /// Description: Here is an example of how to use IPlayerPacketHandlerComponent. You contract the interface and then use ProcessPacket to do the work.
    /// </summary>
    internal class PlayerRotatePacketHandler : IPlayerPacketHandlerComponent
    {
        private CoopGameComponent CoopGameComponent { get { CoopGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
        public ConcurrentDictionary<string, EFT.Player> Players => CoopGameComponent.Players;

        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        public PlayerRotatePacketHandler()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerRotatePacketHandler));
        }

        public void ProcessPacket(Dictionary<string, object> packet)
        {
            string profileId = null;
            string method = null;

            if (!packet.ContainsKey("profileId"))
                return;

            profileId = packet["profileId"].ToString();

            if (!packet.ContainsKey("m"))
                return;

            method = packet["m"].ToString();

            // Now Process dependant on the Packet
            // e.g. if (packet["m"].ToString() == "MoveOperation")
            if (method != "PlayerRotate")
                return;

            //Logger.LogInfo(packet.SITToJson());

            var x = float.Parse(packet["x"].ToString());
            var y = float.Parse(packet["y"].ToString());
            Vector2 rot = new Vector2(x, y);
            ((CoopPlayer)Players[profileId]).ReceiveRotate(rot, false);
        }

    }
}