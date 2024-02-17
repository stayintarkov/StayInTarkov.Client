using EFT;
using System.Reflection;

namespace StayInTarkov.Coop.AI
{
    /// <summary>
    /// Goal: patch removes the current bot from its own enemy list - occurs when adding bots type to its enemy array in difficulty settings
    /// </summary>
    internal class BotSelfEnemyPatch : ModulePatch
    {
        private static readonly string methodName = "PreActivate";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner).GetMethod(methodName);
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner __instance, BotsGroup group)
        {
            IPlayer selfToRemove = null;

            foreach (var enemy in group.Enemies)
            {
                if (enemy.Key.Id == __instance.Id)
                {
                    selfToRemove = enemy.Key;
                    break;
                }
            }

            if (selfToRemove != null)
            {
                group.Enemies.Remove(selfToRemove);
            }

            return true;
        }
    }
}
