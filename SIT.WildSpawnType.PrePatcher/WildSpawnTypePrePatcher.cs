using BepInEx.Logging;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace StayInTarkov
{
    public static class WildSpawnTypePrePatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static int sptUsecValue = 100;
        public static int sptBearValue = 101;

        public static void Patch(ref AssemblyDefinition assembly)
        {
            var logger = Logger.CreateLogSource(nameof(WildSpawnTypePrePatcher));
            logger.LogDebug("Patch!");
            var botEnums = assembly.MainModule.GetType("EFT.WildSpawnType");

            var sptUsec = new FieldDefinition("sptUsec",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
            { Constant = sptUsecValue };

            var sptBear = new FieldDefinition("sptBear",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
            { Constant = sptBearValue };

            if (!botEnums.Fields.Any(x => x.Name == "sptUsec"))
            {
                botEnums.Fields.Add(sptUsec);
                logger.LogDebug($"Added {sptUsec} to {botEnums}");
            }

            if (!botEnums.Fields.Any(x => x.Name == "sptBear"))
            {
                botEnums.Fields.Add(sptBear);
                logger.LogDebug($"Added {sptBear} to {botEnums}");
            }
        }
    }
}