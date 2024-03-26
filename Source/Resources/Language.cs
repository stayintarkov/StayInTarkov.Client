using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace StayInTarkov;

public abstract class LanguageList
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LanguageList));

    /// <summary>
    /// All support language enums. Ensure that the language file name matches the enumeration name.
    /// ATTENTION: THE FIRST ENUMERATION IS THE DEFAULT LANGUAGE.
    /// </summary>
    public enum Language
    {
        [Description("English"), CultureName("en")]
        English,

        [Description("French"), CultureName("fr")]
        French,

        [Description("German"), CultureName("de")]
        German,

        [Description("Japanese"), CultureName("ja")]
        Japanese,

        [Description("Simplified Chinese"), CultureName("zh-CN")]
        SimplifiedChinese,

        [Description("Traditional Chinese"), CultureName("zh-TW")]
        TraditionalChinese,
    }

    /// <summary>
    /// Extension attribute of Language enumeration
    /// </summary>
    /// <param name="name">the Culture Name, such as en, zh-CN</param>
    private class CultureName(string name) : Attribute
    {
        public override string ToString()
        {
            return name;
        }
    }

    private static string GetAttribute<T>(Enum @enum, string def = "") where T : Attribute
    {
        var attributeType = typeof(T);
        var enumType = @enum.GetType();
        var memberInfos = enumType.GetMember(@enum.ToString());
        foreach (var memberInfo in memberInfos)
        {
            var customAttribute = memberInfo.GetCustomAttribute(attributeType, false);
            if (customAttribute != null)
            {
                return customAttribute.ToString();
            }
        }

        return def;
    }

    public static Language ByCultureName(string cultureName)
    {
        var languages = typeof(Language).GetEnumValues();

        foreach (Language language in languages)
        {
            if (string.Equals(GetAttribute<CultureName>(language), cultureName,
                    StringComparison.CurrentCultureIgnoreCase))
            {
                return language;
            }
        }

        return languages.Cast<Language>().First();
    }

    public static string FileName(Language? language)
    {
        language ??= typeof(Language).GetEnumValues().Cast<Language>().First();

        return language + ".json";
    }
}