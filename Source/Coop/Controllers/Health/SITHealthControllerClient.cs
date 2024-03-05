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

        public SITHealthControllerClient(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager)
            : base(healthInfo, player, inventoryController, skillManager, true)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITHealthControllerClient));
            BepInLogger.LogInfo(nameof(SITHealthControllerClient));
        }


        public override void SetEncumbered(bool encumbered)
        {
        }

        public override void SetOverEncumbered(bool encumbered)
        {
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(ApplyItem)}");
            return base.ApplyItem(item, bodyPart, amount);
        }

        public override bool CanApplyItem(Item item, EBodyPart bodyPart)
        {
            BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(CanApplyItem)}");
            return base.CanApplyItem(item, bodyPart);
        }

        public override void CancelApplyingItem()
        {
        }

        protected override void AddEffectToList(AbstractEffect effect)
        {
            base.AddEffectToList(effect);

            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(AddEffectToList)}");

        }

        public override void AddFatigue()
        {
        }

        protected override bool TryGetBodyPartToApply(Item item, EBodyPart bodyPart, out EBodyPart? damagedBodyPart)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(TryGetBodyPartToApply)}");

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
                BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(RemoveEffectFromList)}");

            return base.RemoveEffectFromList(effect);
        }

        public void ReceiveEffect(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(SITHealthControllerClient)}:{nameof(ReceiveEffect)}:{effect}");

            //base.AddEffectToList(effect);
        }

    }


}
