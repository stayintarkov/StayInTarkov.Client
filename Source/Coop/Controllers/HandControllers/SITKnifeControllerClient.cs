using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public sealed class SITKnifeControllerClient : EFT.Player.KnifeController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITKnifeControllerClient));

        public override void Spawn(float animationSpeed, Action callback)
        {
            BepInLogger.LogDebug($"{nameof(SITKnifeControllerClient)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITKnifeControllerClient)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITKnifeControllerClient)}:{nameof(Drop)}");
        }

        public override void BrakeCombo()
        {
            base.BrakeCombo();
        }

        public override void ContinueCombo()
        {
            base.ContinueCombo();
        }

        public override void ExamineWeapon()
        {
            base.ExamineWeapon();
        }

        public override bool MakeAlternativeKick()
        {
            return base.MakeAlternativeKick();
        }

        public override bool MakeKnifeKick()
        {
            return base.MakeKnifeKick();
        }

        public override void Loot(bool p)
        {
            base.Loot(p);
        }

        public override void Pickup(bool p)
        {
            base.Pickup(p);
        }
    }
}
