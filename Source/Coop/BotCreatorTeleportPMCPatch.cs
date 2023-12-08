using EFT;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop
{
    public class BotCreatorTeleportPMCPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(typeof(BotCreator), false)
                 .Single(x =>
                  x.GetParameters().Length > 2
                  && x.GetParameters()[0].Name == "zone"
                  && x.GetParameters()[1].ParameterType == typeof(BotOwner)
                  && x.GetParameters()[1].Name == "bot"
                  );

        }

        [PatchPostfix]
        public static void Postfix(BotZone zone, BotOwner bot)
        {
            EFT.Player getPlayer = bot.GetPlayer;
            GetLogger(typeof(BotCreatorTeleportPMCPatch)).LogInfo("Postfix");

            if (bot.Profile.Info.Settings != null)
            {
                if (bot.Profile.Info.Settings.Role == (WildSpawnType)0x27
                    || bot.Profile.Info.Settings.Role == (WildSpawnType)0x26
                    )
                {
                    GetLogger(typeof(BotCreatorTeleportPMCPatch)).LogInfo("PMC teleporting!");

                }
            }
        }
    }
}
