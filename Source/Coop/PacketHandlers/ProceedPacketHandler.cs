using SIT.Core.Coop.PacketHandlers;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.PacketHandlers
{
    internal class ProceedPacketHandler : IPlayerPacketHandler
    {
        private CoopGameComponent CoopGameComponent { get { CoopGameComponent.TryGetCoopGameComponent(out var coopGC); return coopGC; } }
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;

        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        private HashSet<string> _processedPackets { get; } = new HashSet<string>();

        public ProceedPacketHandler()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ProceedPacketHandler));
        }

        public void ProcessPacket(Dictionary<string, object> packet)
        {
            string profileId = null;

            if (!packet.ContainsKey("profileId"))
                return;

            profileId = packet["profileId"].ToString();

            if (!packet.ContainsKey("m"))
                return;

            //var packetJson = packet.ToJson();
            //if (_processedPackets.Contains(packetJson))
            //    return;

            //_processedPackets.Add(packetJson);

            //Logger.LogInfo(packetJson.ToString());
        }

        public void ProcessPacket(byte[] packet)
        {
        }
    }
}
