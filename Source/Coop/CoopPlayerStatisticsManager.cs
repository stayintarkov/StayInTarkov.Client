using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;

namespace SIT.Core.Coop
{
    internal class CoopPlayerStatisticsManager : AStatisticsManagerForPlayer, IStatisticsManager
    {
        public static Profile Profile { get; set; }

        public override void BeginStatisticsSession()
        {
            base.BeginStatisticsSession();

            Profile = base.Profile_0;
        }

        protected override void ShowStatNotification(LocalizationKey localizationKey1, LocalizationKey localizationKey2, int value)
        {
            
        }
    }
}
