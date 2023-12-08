using DrakiaXYZ.BigBrain.Brains;
using DrakiaXYZ.BigBrain.Internal;
using EFT;
using HarmonyLib;
using StayInTarkov;
using System;
using System.Linq;
using System.Reflection;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the bot brain class lazy loader class so we can lazily load our custom logics
     **/
    public class BotBrainCreateLogicNodePatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(BotBrainClass)).Single(x =>
            {
                var parms = x.GetParameters();
                return (parms.Length == 1 && parms[0].ParameterType == typeof(BotLogicDecision) && parms[0].Name == "decision");
            });
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner ___botOwner_0, BotLogicDecision decision, ref object __result)
        {
#if DEBUG
            try
            {
#endif

            int logicIndex = (int)decision;
            if (logicIndex >= BrainManager.START_LOGIC_ID)
            {
                // Get the offset in the logic list
                logicIndex -= BrainManager.START_LOGIC_ID;

                Type logicType = BrainManager.Instance.CustomLogicList[logicIndex];
                CustomLogicWrapper customLogicWrapper = new CustomLogicWrapper(logicType, ___botOwner_0);
                __result = customLogicWrapper;
#if DEBUG
                    Logger.LogDebug($"Setting bot {___botOwner_0.name} logic to {logicType.FullName}");
#endif

                return false;
            }

            return true;

#if DEBUG
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }
#endif
        }
    }
}