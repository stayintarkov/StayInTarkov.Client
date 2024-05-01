using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace StayInTarkov
{
    public static class WildSpawnTypePrePatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static int sptUsecValue = 47;
        public static int sptBearValue = 48;

        public static void Patch(ref AssemblyDefinition assembly)
        {
            var botEnums = assembly.MainModule.GetType("EFT.WildSpawnType");

            var sptUsec = new FieldDefinition("sptUsec",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
                { Constant = sptUsecValue };

            var sptBear = new FieldDefinition("sptBear",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
                { Constant = sptBearValue };

            if(!botEnums.Fields.Any(x => x.Name == "sptUsec"))
                botEnums.Fields.Add(sptUsec);

            if(!botEnums.Fields.Any(x => x.Name == "sptBear"))
                botEnums.Fields.Add(sptBear);
        }
    }
}