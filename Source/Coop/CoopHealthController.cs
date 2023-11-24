﻿using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;

namespace StayInTarkov.Coop
{
    internal class CoopHealthController : PlayerHealthController
    {
        public CoopHealthController(Profile.Health0 healthInfo, EFT.Player player, InventoryController inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            return base.ApplyItem(item, bodyPart, amount);
        }

        protected override void AddEffectToList(AbstractHealthEffect effect)
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
