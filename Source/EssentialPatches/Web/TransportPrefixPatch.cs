using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StayInTarkov.EssentialPatches.Web
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Core/Patches/TransportPrefixPatch.cs
    /// </summary>
    public class TransportPrefixPatch : ModulePatch
    {
        public TransportPrefixPatch()
        {
            try
            {
                var type = StayInTarkovHelperConstants.EftTypes.Where(x => ReflectionHelpers.GetMethodForType(x, "SaveResponseToCache") != null).Single();
                var value = Traverse.Create(type).Field("TransportPrefixes").GetValue<Dictionary<ETransportProtocolType, string>>();
                value[ETransportProtocolType.HTTPS] = "http://";
                value[ETransportProtocolType.WSS] = "ws://";
            }
            catch (Exception ex)
            {
                Logger.LogError($"{nameof(TransportPrefixPatch)}: {ex}");
                throw;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return StayInTarkovHelperConstants.EftTypes.Single(t => t.GetMethods().Any(m => m.Name == "CreateFromLegacyParams"))
                .GetMethod("CreateFromLegacyParams", BindingFlags.Static | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            object __instance,
            ref LegacyParamsStruct legacyParams)
        {
            legacyParams.Url = legacyParams.Url
                .Replace("https://", "")
                .Replace("http://", "");
            return true; // do original method after
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var searchCode = new CodeInstruction(OpCodes.Ldstr, "https://");
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                //Logger.LogError($"{nameof(TransportPrefixPatch)} failed: Could not find reference code.");
                return instructions;
            }

            codes[searchIndex] = new CodeInstruction(OpCodes.Ldstr, "http://");

            return codes.AsEnumerable();
        }
    }

    public class TransportPrefix2Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(StayInTarkovHelperConstants.EftTypes.SingleOrDefault(t
                => !t.IsInterface
                && ReflectionHelpers.GetAllMethodsForType(t).Any(x => x.Name == "SetUri")
                ), "SetUri", true);
        }

        [PatchPrefix]
        private static bool PatchPrefix(string uri)
        {
            uri
                .Replace("https://", "http://");
            return true;
        }
    }
}