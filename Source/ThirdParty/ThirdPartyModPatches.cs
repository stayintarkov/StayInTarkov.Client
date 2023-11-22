using BepInEx;
using BepInEx.Logging;
using StayInTarkov.UI;

namespace StayInTarkov.ThirdParty
{
    public class ThirdPartyModPatches
    {
        private static BepInEx.Configuration.ConfigFile m_Config;
        public static ManualLogSource Logger { get; private set; }

        static string ConfigSITOtherCategoryValue { get; } = "ThirdParty";

        public static void Run(BepInEx.Configuration.ConfigFile config, BaseUnityPlugin plugin)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Third Party Mods");

            var enabled = config.Bind<bool>(ConfigSITOtherCategoryValue, "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all Other Patches stuff.
            {
                Logger.LogInfo("Third Party patches have been disabled! Ignoring Patches.");
                return;
            }

            if (config.Bind<bool>(ConfigSITOtherCategoryValue, "EnableAdditionalAmmoUIDescriptions", true).Value)
                new Ammo_CachedReadOnlyAttributes_Patch().Enable();

        }
    }
}
