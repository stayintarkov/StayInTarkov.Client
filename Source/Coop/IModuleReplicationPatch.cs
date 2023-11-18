using System;

namespace SIT.Core.Coop
{
    public interface IModuleReplicationPatch
    {
        public abstract Type InstanceType { get; }
        public abstract string MethodName { get; }
        public bool DisablePatch { get; }
    }
}
