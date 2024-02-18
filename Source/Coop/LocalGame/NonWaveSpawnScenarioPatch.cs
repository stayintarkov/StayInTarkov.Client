using BepInEx.Configuration;
using EFT;
using StayInTarkov.Configuration;
using System.Reflection;

namespace StayInTarkov.Coop.LocalGame
{
    internal class NonWaveSpawnScenarioPatch : ModulePatch
    {
        private static ConfigFile _config;

        public NonWaveSpawnScenarioPatch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario), "Run");
        }


        [PatchPrefix]
        public static bool PatchPrefix(NonWavesSpawnScenario __instance)
        {
            var result = !Matchmaker.SITMatchmaking.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "Enabled", result);
            return result;
        }
    }
}
