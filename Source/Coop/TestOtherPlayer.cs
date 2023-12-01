using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.AssetsManager;
using EFT;
using EFT.NextObservedPlayer;
using UnityEngine;
using static RootMotion.FinalIK.InteractionObject;
using RootMotion.FinalIK;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.PrefabSettings;

namespace StayInTarkov.Coop
{
    internal class TestOtherPlayer2 : ObservedPlayerView
    {
        TestOtherPlayer2()
        {
            enabled = true;
        }
    }
}
