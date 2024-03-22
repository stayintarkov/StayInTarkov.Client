using System;
using System.ComponentModel;
using System.Linq;
using BepInEx.Logging;

namespace StayInTarkov;

public abstract class LanguageList
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LanguageList));

    private static readonly LanguageInfo English = new(Language.English, "en", "English.json");
    private static readonly LanguageInfo French = new(Language.French, "fr", "French.json");
    private static readonly LanguageInfo German = new(Language.German, "de", "German.json");
    private static readonly LanguageInfo Japanese = new(Language.Japanese, "ja", "Japanese.json");

    private static readonly LanguageInfo SimplifiedChinese =
        new(Language.SimplifiedChinese, "zh-CN", "SimplifiedChinese.json");

    private static readonly LanguageInfo TraditionalChinese =
        new(Language.TraditionalChinese, "zh-TW", "TraditionalChinese.json");

    public static readonly LanguageInfo Default = English;

    public static readonly LanguageInfo[] Languages =
    [
        English, French, German, Japanese, SimplifiedChinese, TraditionalChinese
    ];

    public enum Language
    {
        [Description("English")] English,
        [Description("French")] French,
        [Description("German")] German,
        [Description("Japanese")] Japanese,
        [Description("Simplified Chinese")] SimplifiedChinese,
        [Description("Traditional Chinese")] TraditionalChinese,
    }

    public class LanguageInfo
    {
        public string CultureName { get; }

        public string FileName { get; }

        public Language Language { get; }

        internal LanguageInfo(Language language, string cultureName, string fileName)
        {
            Language = language;
            CultureName = cultureName;
            FileName = fileName;
        }

        public bool Equals(LanguageInfo other)
        {
            return other != null && other.Language == Language;
        }
    }

    public static LanguageInfo ByLanguage(Language language)
    {
        return Languages.Single(languageInfo => languageInfo.Language == language);
    }

    public static Language ByCultureName(string cultureName)
    {
        try
        {
            return Languages.Single(language => string.Equals(language.CultureName, cultureName,
                StringComparison.CurrentCultureIgnoreCase)).Language;
        }
        catch (Exception)
        {
            Logger.LogWarning($"SIT.Localization fallback to {Default.Language}");
            return Default.Language;
        }
    }
}