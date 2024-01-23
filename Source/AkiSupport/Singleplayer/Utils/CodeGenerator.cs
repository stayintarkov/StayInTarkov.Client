using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils
{
    public class CodeGenerator
        {
            public static List<CodeInstruction> GenerateInstructions(List<Code> codes)
            {
                var list = new List<CodeInstruction>();

                foreach (Code code in codes)
                {
                    list.Add(ParseCode(code));
                }

                return list;
            }

            private static CodeInstruction ParseCode(Code code)
            {
                if (!code.HasOperand)
                {
                    return new CodeInstruction(code.OpCode) { labels = GetLabelList(code) };
                }

                if (code.OpCode == OpCodes.Ldfld || code.OpCode == OpCodes.Stfld)
                {
                    return new CodeInstruction(code.OpCode, AccessTools.Field(code.CallerType, code.OperandTarget as string)) { labels = GetLabelList(code) };
                }

                if (code.OpCode == OpCodes.Call || code.OpCode == OpCodes.Callvirt)
                {
                    return new CodeInstruction(code.OpCode, AccessTools.Method(code.CallerType, code.OperandTarget as string, code.Parameters)) { labels = GetLabelList(code) };
                }

                if (code.OpCode == OpCodes.Box)
                {
                    return new CodeInstruction(code.OpCode, code.CallerType) { labels = GetLabelList(code) };
                }

                if (code.OpCode == OpCodes.Br || code.OpCode == OpCodes.Brfalse || code.OpCode == OpCodes.Brtrue || code.OpCode == OpCodes.Brtrue_S
                    || code.OpCode == OpCodes.Brfalse_S || code.OpCode == OpCodes.Br_S)
                {
                    return new CodeInstruction(code.OpCode, code.OperandTarget) { labels = GetLabelList(code) };
                }

                throw new ArgumentException($"Code with OpCode {nameof(code.OpCode)} is not supported.");
            }

            private static List<Label> GetLabelList(Code code)
            {
                if (code.GetLabel() == null)
                {
                    return new List<Label>();
                }

                return new List<Label>() { (Label)code.GetLabel() };
            }
        }
    
}

