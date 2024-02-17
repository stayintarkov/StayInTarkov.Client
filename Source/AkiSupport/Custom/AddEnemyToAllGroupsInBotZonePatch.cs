using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/BotDifficultyPatch.cs
    /// </summary>
    public class AddEnemyToAllGroupsInBotZonePatch : ModulePatch
    {
        private static Type _targetType;
        private const string methodName = "AddEnemyToAllGroupsInBotZone";

        public AddEnemyToAllGroupsInBotZonePatch()
        {
            _targetType = StayInTarkovHelperConstants.EftTypes.Single(IsTargetType);
        }

        private bool IsTargetType(Type type)
        {
            if (type.Name == nameof(BotsController) && type.GetMethod(methodName) != null)
            {
                return true;
            }

            return false;
        }

        protected override MethodBase GetTargetMethod()
        {
            Logger.LogDebug($"{this.GetType().Name} Type: {_targetType.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {methodName}");

            return _targetType.GetMethod(methodName);
        }

        /// <summary>
        /// AddEnemyToAllGroupsInBotZone()
        /// Goal: by default, AddEnemyToAllGroupsInBotZone doesn't check if the bot group is on the same side as the player.
        /// The effect of this is that when you are a Scav and kill a Usec, every bot group in the zone will aggro you including other Scavs.
        /// This should fix that.
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsController __instance, IPlayer aggressor, IPlayer groupOwner, IPlayer target)
        {
            BotZone botZone = groupOwner.AIData.BotOwner.BotsGroup.BotZone;
            foreach (var item in __instance.Groups())
            {
                if (item.Key != botZone)
                {
                    continue;
                }

                foreach (var group in item.Value.GetGroups(notNull: true))
                {
                    bool differentSide = aggressor.Side != group.Side;
                    bool sameSide = aggressor.Side == target.Side;

                    if (!group.Enemies.ContainsKey(aggressor)
                        && (differentSide || !sameSide)
                        && !group.HaveMemberWithRole(WildSpawnType.gifter)
                        && group.ShallRevengeFor(target)
                        )
                    {
                        group.AddEnemy(aggressor, EBotEnemyCause.AddEnemyToAllGroupsInBotZone);
                    }
                }
            }

            return false;
        }
    }
}
