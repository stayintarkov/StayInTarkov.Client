using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;

namespace StayInTarkov.Coop
{
    internal sealed class CoopHealthController : PlayerHealthController
    {
        public CoopHealthController(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryController inventoryController, SkillManager skillManager, bool aiHealth)
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

        public void AddNetworkEffect(string type, EBodyPart bodyPart, float? delayTime = null, float? workTime = null, float? residueTime = null, float? strength = null)
        {
            // TODO: Oneliner by getting the type string as a class/type

            switch (type)
            {
                case "Berserk":
                    AddEffect<Berserk>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "PainKiller":
                    AddEffect<PainKiller>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "LightBleeding":
                    AddEffect<LightBleeding>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "HeavyBleeding":
                    AddEffect<HeavyBleeding>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "BodyTemperature":
                    AddEffect<BodyTemperature>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "ChronicStaminaFatigue":
                    AddEffect<ChronicStaminaFatigue>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Contusion":
                    AddEffect<Contusion>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "DamageModifier":
                    AddEffect<DamageModifier>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Dehydration":
                    AddEffect<Dehydration>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Disorientation":
                    AddEffect<Disorientation>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Encumbered":
                    AddEffect<Encumbered>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Exhaustion":
                    AddEffect<Exhaustion>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Existence":
                    AddEffect<Existence>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Flash":
                    AddEffect<Flash>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Fracture":
                    AddEffect<Fracture>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "FullHealthRegenerationEffect":
                    AddEffect<FullHealthRegenerationEffect>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "HealthBoost":
                    AddEffect<HealthBoost>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "ImmunityPreventedNegativeEffect":
                    AddEffect<ImmunityPreventedNegativeEffect>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Intoxication":
                    AddEffect<Intoxication>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "LethalIntoxication":
                    AddEffect<LethalIntoxication>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "OverEncumbered":
                    AddEffect<OverEncumbered>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Pain":
                    AddEffect<Pain>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "RadExposure":
                    AddEffect<RadExposure>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Regeneration":
                    AddEffect<Regeneration>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "SandingScreen":
                    AddEffect<SandingScreen>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "ScavRegeneration":
                    AddEffect<ScavRegeneration>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Stun":
                    AddEffect<Stun>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Tremor":
                    AddEffect<Tremor>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "TunnelVision":
                    AddEffect<TunnelVision>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                case "Wound":
                    AddEffect<Wound>(bodyPart, delayTime, workTime, residueTime, strength);
                    break;
                //case "MedEffect":
                //    AddEffect<MedEffect>(bodyPart, delayTime, workTime, residueTime, strength);
                //    break;
            }
        }
    }
}
