using EFT.UI.Matchmaker;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Matchmaker.MatchmakerAccept
{
    public class AcceptInvitePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchMakerAcceptScreen).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).LastOrDefault(
                x =>
                x.GetParameters().Length > 0 &&
                x.GetParameters()[0].Name == "invite");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref object invite)
        {
            Logger.LogInfo("AcceptInvitePatch.PatchPostfix");
            SITMatchmaking.MatchingType = EMatchmakerType.GroupPlayer;
            //MatchmakerAcceptPatches.SetGroupId(ReflectionHelpers.GetFieldOrPropertyFromInstance<string>(invite, "From"));
        }
    }
}
