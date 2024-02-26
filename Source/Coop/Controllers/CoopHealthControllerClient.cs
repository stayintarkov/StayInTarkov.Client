using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

namespace StayInTarkov.Coop.Controllers
{
    internal sealed class CoopHealthControllerClient : ActiveHealthController
    {
        ManualLogSource BepInLogger { get; }

        public CoopHealthControllerClient(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, inventoryController, skillManager)
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
            return true;
        }

        public override void CancelApplyingItem()
        {
        }

        protected override void AddEffectToList(AbstractEffect effect)
        {
        }

        public override void AddFatigue()
        {
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
