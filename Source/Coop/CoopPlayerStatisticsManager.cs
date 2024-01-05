using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using static Ragfair;
using UnityEngine;

namespace StayInTarkov.Coop
{
    public class CoopPlayerStatisticsManager : AStatisticsManagerForPlayer, IStatisticsManager
    {
    
        public override void BeginStatisticsSession()
        {
            //if (coroutine_0 != null)
            //{
            //    StaticManager.KillCoroutine(coroutine_0);
            //}
            //coroutine_0 = StaticManager.BeginCoroutine(method_10());
            ProfileStats eftStats = base.Profile_0.EftStats;
            //eftStats.LastSessionDate = GClass1292.UtcNowUnixInt;
            //dateTime_0 = GClass1292.UtcNow;
            eftStats.Victims.Clear();
            eftStats.Aggressor = null;
            eftStats.SessionExperienceMult = 0f;
            eftStats.ExperienceBonusMult = 0f;
            eftStats.TotalSessionExperience = 0;
            //hashSet_0.Clear();
            //hashSet_1.Clear();
            //method_20(player_0.Profile.Inventory);
            //base.BeginStatisticsSession();
            //dictionary_0 = method_19();
            //player_0.OnPlayerDead += method_17;
            //player_0.OnSpecialPlaceVisited += method_16;
            //player_0.GClass2754_0.OnItemFound += method_13;
            //action_1 = GClass2908.Instance.SubscribeOnEvent<InvokedEvent3>(method_14);
            player_0.GClass2755_0.OnItemFound += (Item i) => { 
            
            };

            if (eftStats.SessionCounters == null)
            {
                eftStats.SessionCounters = new OverallAccountStats();
            }
            else
            {
                eftStats.SessionCounters.Clear();
            }
            if (eftStats.DamageHistory == null)
            {
                eftStats.DamageHistory = new DamageHistory();
            }
            else
            {
                eftStats.DamageHistory.Clear();
            }
            StartDamageHistory();
        }

      

   

     

        public override void ShowStatNotification(LocalizationKey localizationKey1, LocalizationKey localizationKey2, int value)
        {
            if (value > 0)
            {
                NotificationManagerClass.DisplayNotification(new AbstractNotification46(localizationKey1, localizationKey2, value));
            }
        }
    }
}
