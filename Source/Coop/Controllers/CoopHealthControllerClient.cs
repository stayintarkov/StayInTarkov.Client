using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers
{
    internal sealed class CoopHealthControllerClient : PlayerHealthController
    {
        ManualLogSource BepInLogger { get; }

        public CoopHealthControllerClient(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopHealthControllerClient));
            BepInLogger.LogInfo(nameof(CoopHealthControllerClient));    
        }


        public override void SetEncumbered(bool encumbered)
        {
            base.SetEncumbered(encumbered);
        }

        public override void SetOverEncumbered(bool encumbered)
        {
            base.SetOverEncumbered(encumbered);
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            return base.ApplyItem(item, bodyPart, amount);
        }

        protected override void AddEffectToList(AbstractEffect effect)
        {
            base.AddEffectToList(effect);
        }

        public new void Kill(EDamageType damageType)
        {
            BepInLogger.LogInfo(nameof(Kill));
        }

        public new float ApplyDamage(EBodyPart bodyPart, float damage, DamageInfo damageInfo)
        {
            BepInLogger.LogInfo(nameof(ApplyDamage));
            return 0;
        }

        public new void HandleFall(float height)
        {
            BepInLogger.LogInfo(nameof(HandleFall));
        }


    }
}
