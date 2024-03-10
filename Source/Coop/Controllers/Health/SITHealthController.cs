using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace StayInTarkov.Coop.Controllers.Health
{
    public sealed class SITHealthController : PlayerHealthController
    {
        public ManualLogSource BepInLogger { get; } = Logger.CreateLogSource(nameof(SITHealthController));
        public ConcurrentQueue<PlayerHealthEffectPacket> PlayerHealthEffectPackets { get; } = new();

        /// <summary>
        /// This system relies on EffectMakerPatch and EffectResolvePatch. Please DO NOT remove them!
        /// </summary>
        public override bool _sendNetworkSyncPackets { get { return true; } }

        public EFT.Player _Player;

        public SITHealthController(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
            BepInLogger.LogDebug(this.GetType().Name);
            _Player = player;
        }

        /// <summary>
        /// Ignore the NetworkSyncPacket from BSG and create our own to send
        /// </summary>
        /// <param name="packet"></param>
        public override void SendNetworkSyncPacket(HealthSyncPacket packet)
        {
            try 
            {
                //if (BepInLogger != null)
                //    BepInLogger.LogDebug($"{this.GetType().Name}:{nameof(SendNetworkSyncPacket)}");

                if (_Player == null || _Player.ProfileId == null)
                {
                    if (BepInLogger != null)
                        BepInLogger.LogDebug($"{this.GetType().Name}:{nameof(SendNetworkSyncPacket)}:Player or ProfileId is null?");
                    return;
                }

                PlayerHealthEffectPacket healthPacket = new (_Player.ProfileId);
                PlayerHealthEffectPackets.Enqueue(healthPacket);
            }
            catch(Exception ex)
            {
                if (BepInLogger != null)
                    BepInLogger.LogError($"{nameof(SendNetworkSyncPacket)}:{ex}");
            }
        }


    }
}
