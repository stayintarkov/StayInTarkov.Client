using EFT;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.AI;
using StayInTarkov.Networking;
using System.Reflection;

namespace StayInTarkov.Coop.AI
{
    /// <summary>
    /// Goal: Despawning bots between Server and Client is broken, this patch aims to fix that by patching DestroyInfo to send a packet once it has ran.
    /// </summary>
    internal class BotDespawnPatch : ModulePatch
    {
        private static readonly string methodName = "DestroyInfo";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsController).GetMethod(methodName);
        }

        [PatchPrefix]
        private static bool PatchPrefix(EFT.Player player)
        {
            return false;
        }

        [PatchPostfix]
        public static void Postfix(EFT.Player player)
        {
            if (SITMatchmaking.IsClient)
                return;

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            coopGameComponent.ProfileIdsAI.Remove(player.ProfileId);

            DespawnAIPacket packet = new();
            packet.AIProfileId = player.ProfileId;

            GameClient.SendData(packet.Serialize());
        }
    }
}
