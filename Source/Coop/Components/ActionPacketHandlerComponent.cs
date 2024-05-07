using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Players;
using System.Collections.Concurrent;
using UnityEngine;

namespace StayInTarkov.Coop.Components
{
    public class ActionPacketHandlerComponent : MonoBehaviour
    {
        public readonly BlockingCollection<ISITPacket> ActionSITPackets = new(9999);
        public ConcurrentDictionary<string, CoopPlayer> Players => CoopGameComponent.Players;
        public ManualLogSource Logger { get; private set; }

        private SITGameComponent CoopGameComponent { get; set; }

        void Awake()
        {
            // ----------------------------------------------------
            // Create a BepInEx Logger for ActionPacketHandlerComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource("ActionPacketHandlerComponent");
            Logger.LogDebug("Awake");
        }

        void Start()
        {
            CoopGameComponent = this.gameObject.GetComponent<SITGameComponent>();
        }

        void Update()
        {
            ProcessActionPackets();
        }

        private void ProcessActionPackets()
        {
            CoopGameComponent = gameObject.GetComponent<SITGameComponent>();

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (Players == null)
                return;

            if (ActionSITPackets.Count > 0)
            {
#if DEBUGPACKETS
                Stopwatch stopwatchActionPackets = Stopwatch.StartNew();
#endif
                while (ActionSITPackets.TryTake(out var packet))
                {
#if DEBUGPACKETS
                    Stopwatch stopwatchActionPacket = Stopwatch.StartNew();
#endif
                    packet.Process();

#if DEBUGPACKETS
                    if (stopwatchActionPacket.ElapsedMilliseconds > 1)
                        Logger.LogDebug($"ActionSITPacket {packet.Method} took {stopwatchActionPacket.ElapsedMilliseconds}ms to process!");
#endif
                }
#if DEBUGPACKETS
                if (stopwatchActionPackets.ElapsedMilliseconds > 1)
                    Logger.LogDebug($"ActionSITPackets took {stopwatchActionPackets.ElapsedMilliseconds}ms to process!");
#endif
            }

            return;
        }
    }
}
