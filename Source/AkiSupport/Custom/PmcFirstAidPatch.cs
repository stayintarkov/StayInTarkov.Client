using EFT;
using StayInTarkov;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// SPT PMC enum value is high enough in wildspawntype it means the first aid class that gets init doesnt have an implementation
    /// On heal event, remove all negative effects from limbs e.g. light/heavy bleeds
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/PmcFirstAidPatch.cs
    /// </summary>
    public class PmcFirstAidPatch : ModulePatch
    {
        private static Type _targetType;
        private static readonly string methodName = "FirstAidApplied";

        public PmcFirstAidPatch()
        {
            _targetType = StayInTarkovHelperConstants.EftTypes.FirstOrDefault(IsTargetType);
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// GCLass350 for client version 25782
        /// </summary>
        private bool IsTargetType(Type type)
        {
            if (type.GetMethod("GetHpPercent") != null && type.GetMethod("TryApplyToCurrentPart") != null)
            {
                return true;
            }

            return false;
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner ___botOwner_0)
        {
            if (___botOwner_0.IsRole((WildSpawnType)0x29L) || ___botOwner_0.IsRole((WildSpawnType)0x2AL))
            {
                var healthController = ___botOwner_0.GetPlayer.ActiveHealthController;

                healthController.RemoveNegativeEffects(EBodyPart.Head);
                healthController.RemoveNegativeEffects(EBodyPart.Chest);
                healthController.RemoveNegativeEffects(EBodyPart.Stomach);
                healthController.RemoveNegativeEffects(EBodyPart.LeftLeg);
                healthController.RemoveNegativeEffects(EBodyPart.RightLeg);
                healthController.RemoveNegativeEffects(EBodyPart.LeftArm);
                healthController.RemoveNegativeEffects(EBodyPart.RightArm);
            }

            return false; // skip original
        }
    }
}