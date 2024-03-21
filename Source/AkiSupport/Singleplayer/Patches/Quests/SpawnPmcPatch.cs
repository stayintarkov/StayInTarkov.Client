using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Singleplayer.Patches.Quests
{
    public class SpawnPmcPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = StayInTarkovHelperConstants.EftTypes.SingleCustom(IsTargetType);
            var desiredMethod = desiredType.GetMethod("method_1", StayInTarkovHelperConstants.PublicDeclaredFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetType(Type type)
        {
            if (!typeof(IGetProfileData).IsAssignableFrom(type) || type.GetMethod("method_1", StayInTarkovHelperConstants.PublicDeclaredFlags) == null)
            {
                return false;
            }

            var fields = type.GetFields(StayInTarkovHelperConstants.PrivateFlags);
            return fields.Any(f => f.FieldType != typeof(WildSpawnType)) && fields.Any(f => f.FieldType == typeof(BotDifficulty));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, WildSpawnType ___wildSpawnType_0, BotDifficulty ___botDifficulty_0, Profile x)
        {
            if (x == null)
            {
                __result = false;
                Logger.LogInfo($"profile x was null, ___wildSpawnType_0 = {___wildSpawnType_0}");
                return false; // Skip original
            }

            __result = x.Info.Settings.Role == ___wildSpawnType_0 && x.Info.Settings.BotDifficulty == ___botDifficulty_0;

            return false; // Skip original
        }
    }
}
