using BepInEx.Logging;
using Diz.LanguageExtensions;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers
{
    internal sealed class CoopHealthControllerClient 
        // Paulov: This should be ActiveHealthController. However, a lot of the patches use PlayerHealthController, need to fix
        : PlayerHealthController
        //: ActiveHealthController
    {
        ManualLogSource BepInLogger { get; }

        public CoopHealthControllerClient(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, false)
            //: base(healthInfo, inventoryController, skillManager)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopHealthControllerClient));
            BepInLogger.LogInfo(nameof(CoopHealthControllerClient));    
        }


        public override void SetEncumbered(bool encumbered)
        {
        }

        public override void SetOverEncumbered(bool encumbered)
        {
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            BepInLogger.LogInfo($"{nameof(CoopHealthControllerClient)}:{nameof(ApplyItem)}");    
            return true;
        }

        public override bool CanApplyItem(Item item, EBodyPart bodyPart)
        {
            BepInLogger.LogInfo($"{nameof(CoopHealthControllerClient)}:{nameof(CanApplyItem)}");    
            return base.CanApplyItem(item, bodyPart);
        }

        public override void CancelApplyingItem()
        {
        }

        protected override void AddEffectToList(AbstractEffect effect)
        {
            //base.AddEffectToList(effect);

            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(CoopHealthControllerClient)}:{nameof(AddEffectToList)}");

        }

        public override void AddFatigue()
        {
        }

        protected override bool TryGetBodyPartToApply(Item item, EBodyPart bodyPart, out EBodyPart? damagedBodyPart)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(CoopHealthControllerClient)}:{nameof(TryGetBodyPartToApply)}");

            damagedBodyPart = bodyPart;
            return base.TryGetBodyPartToApply(item, bodyPart, out damagedBodyPart);

            //if (item is MedsClass medsClass)
            //{
            //    MedsClass medsClass2 = medsClass;
            //    float num = medsClass2.HealthEffectsComponent.UseTimeFor(bodyPart);
            //    if (base.Dictionary_0[bodyPart].IsDestroyed && medsClass2.HealthEffectsComponent.AffectsAny(EDamageEffectType.DestroyedPart))
            //    {
            //        num /= 1f + (float)skillManager_0.SurgerySpeed;
            //    }
            //    AddEffect(bodyPart, 0f, num, null, null, delegate (MedEffect e)
            //    {
            //        e.Init(medsClass2, 1f);
            //    });
            //}
            //if (item is FoodClass foodDrink)
            //{
            //    FoodClass foodDrink2 = foodDrink;
            //    //float actualAmount = amount ?? (foodDrink2.FoodDrinkComponent.HpPercent / foodDrink2.FoodDrinkComponent.MaxResource);
            //    float actualAmount = foodDrink2.FoodDrinkComponent.HpPercent / foodDrink2.FoodDrinkComponent.MaxResource;
            //    AddEffect(bodyPart, 0f, foodDrink2.HealthEffectsComponent.UseTime * actualAmount, null, null, delegate (MedEffect e)
            //    {
            //        e.Init(foodDrink2, actualAmount);
            //    });
            //}
            //return false;
        }

        protected override bool RemoveEffectFromList(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(CoopHealthControllerClient)}:{nameof(RemoveEffectFromList)}");

            return base.RemoveEffectFromList(effect);
        }
    }
}
