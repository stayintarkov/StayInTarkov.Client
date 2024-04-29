using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Player
{
    internal sealed class TraderControllerClassHasForeignEventsPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(TraderControllerClass);

        public override string MethodName => "HasForeignEvents";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool Prefix(bool __result)
        {
            __result = false;
            return false;
        }
    }
}
