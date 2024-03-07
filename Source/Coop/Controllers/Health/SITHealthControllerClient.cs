using BepInEx.Logging;
using Diz.LanguageExtensions;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers.Health
{
    internal sealed class SITHealthControllerClient
        // Paulov: This should be ActiveHealthController. However, a lot of the patches use PlayerHealthController, need to fix
        : PlayerHealthController
    //: ActiveHealthController
    {
        ManualLogSource BepInLogger { get; }

        public override bool _sendNetworkSyncPackets => false;

        public SITHealthControllerClient(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager)
            : base(healthInfo, player, inventoryController, skillManager, true)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITHealthControllerClient));
            BepInLogger.LogInfo(nameof(SITHealthControllerClient));
        }

        public override void SendNetworkSyncPacket(HealthSyncPacket packet)
        {
            // Do nothing
        }


    }


}
