using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StayInTarkov.Coop.PacketHandlers
{
    /// <summary>
    /// Created by Paulov
    /// Description: Here is an example of how to use IPlayerPacketHandlerComponent. You contract the interface and then use ProcessPacket to do the work.
    /// </summary>
    internal class ExamplePlayerPacketHandler : IPlayerPacketHandlerComponent
    {
        private CoopGameComponent CoopGameComponent { get { CoopGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
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

    }
}