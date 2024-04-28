//using EFT;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;

//namespace StayInTarkov.AI
//{
//    /// <summary>
//    /// Paulov: Stay in Tarkov causes an error with the phrase BLOCKER ERROR caused by spt WildSpawnTypes
//    /// This patch resolves this error by adding the spt WildSpawnTypes to the ServerBotSettingsClass
//    /// LICENSE: This patch is only for use in Stay In Tarkov
//    /// </summary>
//    public sealed class BlockerErrorFixPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            return AccessTools.GetDeclaredMethods(typeof(ServerBotSettingsClass)).First(x => x.Name == nameof(ServerBotSettingsClass.Init));
//        }

//        [PatchPrefix]
//        public static bool Prefix(Dictionary<WildSpawnType, ServerBotSettingsValuesClass> ___dictionary_0)
//        {
//            var enumValues = Enum.GetValues(typeof(WildSpawnType));
//            ___dictionary_0.Add((WildSpawnType)enumValues.GetValue(enumValues.Length - 2), new ServerBotSettingsValuesClass(false, false, true, "ScavRole/PmcBot"));
//            ___dictionary_0.Add((WildSpawnType)enumValues.GetValue(enumValues.Length - 1), new ServerBotSettingsValuesClass(false, false, true, "ScavRole/PmcBot"));
//            return true;
//        }
//    }
//}
