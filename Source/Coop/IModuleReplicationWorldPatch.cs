using System;
using System.Collections.Generic;

namespace StayInTarkov.Coop
{
    internal interface IModuleReplicationWorldPatch
    {
        public Type InstanceType { get; }
        public string MethodName { get; }
        public bool DisablePatch { get; }

        public void Replicated(ref Dictionary<string, object> packet);

    }
}
