using System;
using System.Reflection.Emit;

namespace SIT.Tarkov.Core
{
    public class Code
    {
        public OpCode OpCode { get; }
        public Type CallerType { get; }
        public object OperandTarget { get; }
        public Type[] Parameters { get; }
        public bool HasOperand { get; }

        public Code(OpCode opCode)
        {
            OpCode = opCode;
            HasOperand = false;
        }

        public Code(OpCode opCode, object operandTarget)
        {
            OpCode = opCode;
            OperandTarget = operandTarget;
            HasOperand = true;
        }

        public Code(OpCode opCode, Type callerType)
        {
            OpCode = opCode;
            CallerType = callerType;
            HasOperand = true;
        }

        public Code(OpCode opCode, Type callerType, object operandTarget, Type[] parameters = null)
        {
            OpCode = opCode;
            CallerType = callerType;
            OperandTarget = operandTarget;
            Parameters = parameters;
            HasOperand = true;
        }

        public virtual Label? GetLabel()
        {
            return null;
        }
    }
}
