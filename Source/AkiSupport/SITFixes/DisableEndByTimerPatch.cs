using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.AkiSupport.SITFixes
{
    /// <summary>
    /// TODO: One of the many patches we will need to remove when we start full Aki support
    /// </summary>
    internal sealed class DisableEndByTimerPatch : ModulePatch
    {
        public DisableEndByTimerPatch()
        {
        }

        protected override MethodBase GetTargetMethod()
        {
            return null;
        }
    }
}
