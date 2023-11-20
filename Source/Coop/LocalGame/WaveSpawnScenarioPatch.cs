using BepInEx.Configuration;
using EFT;
using StayInTarkov.Configuration;
using System.Reflection;

namespace StayInTarkov.Coop.LocalGame
{
    internal class WaveSpawnScenarioPatch : ModulePatch
    {
        private static ConfigFile _config;

        public WaveSpawnScenarioPatch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "Run");
        }


        [PatchPrefix]
        public static bool PatchPrefix(WavesSpawnScenario __instance)
        {
            var result = !Matchmaker.MatchmakerAcceptPatches.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "Enabled", result);
            return result;
        }
    }
}
