using BepInEx.Logging;
using EFT.NetworkPackets;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GClass2179

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct PrevFrame
    {
        private static ManualLogSource Logger;
        public PrevFrame()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("PrevFrame");
        }

        public EHandsTypePacket HandsTypePacket { get; set; }
        public MovementInfoPacket MovementInfoPacket { get; set; }
        public HandsChangePacket HandsChangePacket { get; set; }
        public HelmetLightPacket HelmetLightPacket { get; set; }
        public TacticalComboPacket TacticalComboPacket { get; set; }
        public List<ICommand> Commands { get; set; } = new List<ICommand>();

        public void ReadFrame()
        {
            if (HelmetLightPacket.LightsStates != null)
            {
                Commands.Add(new Command()
                {
                    SetSilently = HelmetLightPacket.IsSilent,
                    ID = HelmetLightPacket.LightsStates[0].Id,
                    LightMode = HelmetLightPacket.LightsStates[0].LightMode,
                    State = HelmetLightPacket.LightsStates[0].IsActive
                });
                Logger.LogInfo("Added HLP!");
                HelmetLightPacket = default;
            }
        }

        public void ClearFrame()
        {
            Commands.Clear();
        }
    }
}
