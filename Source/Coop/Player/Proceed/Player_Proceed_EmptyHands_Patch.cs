using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_Proceed_EmptyHands_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedEmptyHands";

        public static List<string> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "withNetwork");
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            // Giving 'false' to the player will cause issue.
            // return CallLocally.Contains(__instance.ProfileId);

            return true;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance, bool withNetwork, bool scheduled)
        {
            if (CallLocally.Contains(__instance.ProfileId))
            {
                CallLocally.Remove(__instance.ProfileId);
                return;
            }

            PlayerProceedEmptyHandsPacket playerProceedEmptyHandsPacket = new(__instance.ProfileId, withNetwork, scheduled, "ProceedEmptyHands");
            GameClient.SendData(playerProceedEmptyHandsPacket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            // The original function is always running, don't let it run again.
            if (IsHighPingOwnPlayerOrAI(player))
                return;

            if (!dict.ContainsKey("data"))
                return;

            PlayerProceedEmptyHandsPacket playerProceedEmptyHandsPacket = new(player.ProfileId, true, true, null);
            playerProceedEmptyHandsPacket.Deserialize((byte[])dict["data"]);

            if (HasProcessed(GetType(), player, playerProceedEmptyHandsPacket))
                return;

            CallLocally.Add(player.ProfileId);
            player.Proceed(playerProceedEmptyHandsPacket.WithNetwork, null, playerProceedEmptyHandsPacket.Scheduled);
        }
    }

    public class PlayerProceedEmptyHandsPacket : BasePlayerPacket
    {
        public bool WithNetwork { get; set; }

        public bool Scheduled { get; set; }

        public PlayerProceedEmptyHandsPacket(string profileId, bool withNetwork, bool scheduled, string method) : base(profileId, method)
        {
            WithNetwork = withNetwork;
            Scheduled = scheduled;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(Scheduled);
            writer.Write(WithNetwork);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            Scheduled = reader.ReadBoolean();
            WithNetwork = reader.ReadBoolean();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }
    }
}