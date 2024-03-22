using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.UI
{

    /// <summary>
    /// Paulov. Adding Damage attribute to UI Template
    /// </summary>
    internal class Ammo_CachedReadOnlyAttributes_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(AmmoTemplate), "GetCachedReadonlyQualities");
        }

        [PatchPostfix]
        private static void Postfix(ref AmmoTemplate __instance, ref List<ItemAttributeClass> __result)
        {
            if (!__result.Any((ItemAttributeClass a) => (Attributes.ENewMaximumDurabilityId)a.Id == Attributes.ENewMaximumDurabilityId.Damage))
            {
                AddNewAttributes(ref __result, __instance);
            }
        }

        public static void AddNewAttributes(ref List<ItemAttributeClass> attributes, AmmoTemplate template)
        {
            if (template == null)
                return;

            // Damage
            if (template.Damage > 0)
            {
                attributes.Add(
                    new ItemAttributeClass(Attributes.ENewMaximumDurabilityId.Damage)
                    {
                        Name = Attributes.ENewMaximumDurabilityId.Damage.GetName(),
                        Base = (() => template.Damage),
                        StringValue = (() => template.Damage.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }

            // Armor Damage
            if (template.ArmorDamage > 0)
            {
                attributes.Add(
                    new ItemAttributeClass(Attributes.ENewMaximumDurabilityId.ArmorDamage)
                    {
                        Name = Attributes.ENewMaximumDurabilityId.ArmorDamage.GetName(),
                        Base = (() => template.ArmorDamage),
                        StringValue = (() => template.ArmorDamage.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }

            // Penetration
            if (template.PenetrationPower > 0)
            {
                attributes.Add(
                    new ItemAttributeClass(Attributes.ENewMaximumDurabilityId.Penetration)
                    {
                        Name = Attributes.ENewMaximumDurabilityId.Penetration.GetName(),
                        Base = (() => template.PenetrationPower),
                        StringValue = (() => template.PenetrationPower.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }
        }
    }

    public static class Attributes
    {
        public static string GetName(this Attributes.ENewMaximumDurabilityId id)
        {
            switch (id)
            {
                case Attributes.ENewMaximumDurabilityId.Damage:
                    // return "DAMAGE";
                    return StayInTarkovPlugin.LanguageDictionary["DAMAGE"].ToString();
                case Attributes.ENewMaximumDurabilityId.ArmorDamage:
                    // return "ARMOR DAMAGE";
                    return StayInTarkovPlugin.LanguageDictionary["ARMOR_DAMAGE"].ToString();
                case Attributes.ENewMaximumDurabilityId.Penetration:
                    // return "PENETRATION";
                    return StayInTarkovPlugin.LanguageDictionary["PENETRATION"].ToString();
                case Attributes.ENewMaximumDurabilityId.FragmentationChance:
                    // return "FRAGMENTATION CHANCE";
                    return StayInTarkovPlugin.LanguageDictionary["FRAGMENTATION_CHANCE"].ToString();
                case Attributes.ENewMaximumDurabilityId.RicochetChance:
                    // return "RICOCHET CHANCE";
                    return StayInTarkovPlugin.LanguageDictionary["RICOCHET_CHANCE"].ToString();
                default:
                    return id.ToString();
            }
        }

        public enum ENewMaximumDurabilityId
        {
            Damage,
            ArmorDamage,
            Penetration,
            FragmentationChance,
            RicochetChance
        }
    }
}
