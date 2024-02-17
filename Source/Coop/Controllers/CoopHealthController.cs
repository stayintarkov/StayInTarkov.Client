using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;

namespace StayInTarkov.Coop.Controllers
{
    internal sealed class CoopHealthController : PlayerHealthController
    {
        public CoopHealthController(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            return base.ApplyItem(item, bodyPart, amount);
        }

        protected override void AddEffectToList(AbstractEffect effect)
        {
            base.AddEffectToList(effect);
        }

        public override void SetEncumbered(bool encumbered)
        {
            base.SetEncumbered(encumbered);
        }

        public override void SetOverEncumbered(bool encumbered)
        {
            base.SetOverEncumbered(encumbered);
        }
    }
}
