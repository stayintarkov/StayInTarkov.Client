using DrakiaXYZ.BigBrain.Brains;
using DrakiaXYZ.BigBrain.Internal;
using EFT;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Reflection;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the bot brain class lazy loader class so we can lazily load our custom logics
     **/
    internal class BotBrainCreateLogicNodePatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotBrainClass), "method_0");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner ___botOwner_0, BotLogicDecision decision, ref object __result)
        {
            try
            {

                int logicIndex = (int)decision;
                if (logicIndex >= BrainManager.START_LOGIC_ID)
                {
                    // Get the offset in the logic list
                    logicIndex -= BrainManager.START_LOGIC_ID;

                    Type logicType = BrainManager.Instance.CustomLogicList[logicIndex];
                    CustomLogicWrapper customLogicWrapper = new(logicType, ___botOwner_0);
                    __result = customLogicWrapper;


                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }
        }
    }
}
