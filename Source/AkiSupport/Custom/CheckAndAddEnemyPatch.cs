using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/CheckAndAddEnemyPatch.cs
    /// </summary>
    public class CheckAndAddEnemyPatch : ModulePatch
    {
        private static Type _targetType;
        private static FieldInfo _sideField;
        private static FieldInfo _enemiesField;
        private static FieldInfo _spawnTypeField;
        private static MethodInfo _addEnemy;
        private readonly string _targetMethodName = "CheckAndAddEnemy";

        public CheckAndAddEnemyPatch()
        {
            _targetType = StayInTarkovHelperConstants.EftTypes.Single(IsTargetType);
            _sideField = _targetType.GetField("Side");
            _enemiesField = _targetType.GetField("Enemies");
            _spawnTypeField = _targetType.GetField("wildSpawnType_0", BindingFlags.NonPublic | BindingFlags.Instance);
            _addEnemy = _targetType.GetMethod("AddEnemy");
        }

        private bool IsTargetType(Type type)
        {
            if (type.GetMethod("AddEnemy") != null && type.GetMethod("AddEnemyGroupIfAllowed") != null)
            {
                return true;
            }

            return false;
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod(_targetMethodName);
        }

        /// <summary>
        /// CheckAndAddEnemy()
        /// Goal: This patch lets bosses shoot back once a PMC has shot them
        /// removes the !player.AIData.IsAI  check
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(object __instance, ref IAIDetails player, ref bool ignoreAI)
        {
            if (!player.HealthController.IsAlive)
            {
                return false; // do nothing and skip
            }

            var enemies = (Dictionary<IAIDetails, BotSettingsClass>)_enemiesField.GetValue(__instance);
            if (enemies.ContainsKey(player))
            {
                return false;// do nothing and skip
            }

            // Add enemy to list
            _addEnemy.Invoke(__instance, new object[] { player, EBotEnemyCause.checkAddTODO });

            return false;
        }
    }
}
