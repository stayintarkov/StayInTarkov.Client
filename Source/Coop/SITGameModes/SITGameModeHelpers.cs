using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.SITGameModes
{
    public static class SITGameModeHelpers
    {
        public static void RemoveEndByTriggers()
        {
            if (Singleton<ISITGame>.Instance == null)
                return;

            var game = Singleton<ISITGame>.Instance as BaseLocalGame<GamePlayerOwner>;

            if(game.TryGetComponent<EndByTimerScenario>(out var endByTimerScenario))
                GameObject.DestroyImmediate(endByTimerScenario);

            if (game.TryGetComponent<EndByExitTrigerScenario>(out var endByExitTrigerScenario))
                GameObject.DestroyImmediate(endByExitTrigerScenario);

        }
    }
}
