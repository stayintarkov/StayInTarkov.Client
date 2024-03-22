using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StayInTarkov.Coop.PacketHandlers
{
    /// <summary>
    /// Created by Paulov
    /// Description: Here is an example of how to use IPlayerPacketHandler. You contract the interface and then use ProcessPacket to do the work.
    /// </summary>
    internal class ExamplePlayerPacketHandler : IPlayerPacketHandler
    {
        private SITGameComponent CoopGameComponent { get { SITGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;

        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        public ExamplePlayerPacketHandler()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ExamplePlayerPacketHandler));
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

            //Logger.LogInfo(packet.SITToJson());
        }

        public void ProcessPacket(byte[] packet)
        {
            throw new System.NotImplementedException();
        }
    }
}