using System;

namespace StayInTarkov.Coop
{
    internal interface IModuleReplicationPatch
    {
        public abstract Type InstanceType { get; }
        public abstract string MethodName { get; }
        public bool DisablePatch { get; }
    }
}
