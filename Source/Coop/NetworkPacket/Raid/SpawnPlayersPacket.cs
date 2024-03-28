using Comfort.Common;
using EFT;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers.CoopInventory;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class SpawnPlayersPacket : BasePacket
    {
        public PlayerInformationPacket[] InformationPackets { get; set; } = [];

        public SpawnPlayersPacket() : base(nameof(SpawnPlayersPacket))
        {

        }

        public SpawnPlayersPacket(in PlayerInformationPacket[] informationPackets) : this()
        {
            InformationPackets = informationPackets;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            
            writer.Write(InformationPackets.Length);
            foreach (var packet in InformationPackets)
            {
                writer.WriteLengthPrefixedBytes(packet.Serialize());
            }

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);

            var length = reader.ReadInt32();
            InformationPackets = new PlayerInformationPacket[length];
            for (int i = 0; i < length; i++)
            {
                InformationPackets[i] = new PlayerInformationPacket();
                InformationPackets[i] = (PlayerInformationPacket)InformationPackets[i].Deserialize(reader.ReadLengthPrefixedBytes());
            }

            return this;
        }

        public override void Process()
        {
            // ------------------------------------------------------------------------------------------
            // Receive the Packet
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(SpawnPlayersPacket)}:{nameof(Process)}");
            EFT.UI.ConsoleScreen.Log($"{nameof(SpawnPlayersPacket)}:{nameof(Process)}");
            // ------------------------------------------------------------------------------------------


            var sitGame = Singleton<ISITGame>.Instance as AbstractGame;

            if (!SITGameComponent.TryGetCoopGameComponent(out var sitGameComponent))
                return;

            foreach (var packet in InformationPackets)
            {
                if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.ProfileId == packet.ProfileId))
                    continue;

                var profileId = packet.ProfileId;   

                if (sitGameComponent.PlayersToSpawn.ContainsKey(profileId))
                    continue;

                if (!sitGameComponent.PlayersToSpawnPacket.ContainsKey(profileId))
                    sitGameComponent.PlayersToSpawnPacket.TryAdd(profileId, packet);

                if (!sitGameComponent.PlayersToSpawn.ContainsKey(profileId))
                    sitGameComponent.PlayersToSpawn.TryAdd(profileId, ESpawnState.None);

                if (packet.IsAI)
                    sitGameComponent.Logger.LogDebug($"{nameof(Process)}:isAI:{packet.IsAI}");

                if (packet.IsAI && !sitGameComponent.ProfileIdsAI.Contains(profileId))
                {
                    sitGameComponent.ProfileIdsAI.Add(profileId);
                    sitGameComponent.Logger.LogDebug($"Added AI Character {profileId} to {nameof(sitGameComponent.ProfileIdsAI)}");
                }

                if (!packet.IsAI && !sitGameComponent.ProfileIdsUser.Contains(profileId))
                {
                    sitGameComponent.ProfileIdsUser.Add(profileId);
                    sitGameComponent.Logger.LogDebug($"Added User Character {profileId} to {nameof(sitGameComponent.ProfileIdsUser)}");
                }

            }

        }

        public static void CreateFromGame(string[] excludeIds)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(SpawnPlayersPacket)}.{nameof(CreateFromGame)}");

            List<PlayerInformationPacket> infoPackets = new List<PlayerInformationPacket>();
            foreach (var p in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if (excludeIds.Contains(p.ProfileId))
                    continue;

                var infoPacket = CreateInformationPacketFromPlayer(p);
                infoPackets.Add(infoPacket);
            }

            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(SpawnPlayersPacket)}.{nameof(CreateFromGame)}.Sending {infoPackets.Count} info packets");
            SpawnPlayersPacket packet = new SpawnPlayersPacket(infoPackets.ToArray());
            GameClient.SendData(packet.Serialize());
        }

        public static PlayerInformationPacket CreateInformationPacketFromPlayer(EFT.Player p)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(SpawnPlayersPacket)}.{nameof(CreateInformationPacketFromPlayer)}");

            var infoPacket = new PlayerInformationPacket(p.ProfileId);
            infoPacket.AccountId = p.AccountId;
            infoPacket.BodyPosition = p.Position;
            infoPacket.Customization = p.Profile.Customization;
            infoPacket.GroupID = !string.IsNullOrEmpty(p.GroupId) ? p.GroupId : "";
            infoPacket.Inventory = p.Profile.Inventory;
            infoPacket.IsAI = p.IsAI;
            infoPacket.NickName = !string.IsNullOrEmpty(p.Profile.Nickname) ? p.Profile.Nickname : "";
            infoPacket.RemoteTime = Time.fixedTime;
            infoPacket.Side = p.Side;
            infoPacket.TeamID = !string.IsNullOrEmpty(p.GroupId) ? p.GroupId : "";
            infoPacket.Voice = !string.IsNullOrEmpty(p.Profile.Info.Voice) ? p.Profile.Info.Voice : "";
            infoPacket.VoIPState = EFT.Player.EVoipState.Off;
            infoPacket.WildSpawnType = p.Profile.Info.Settings.Role;
            infoPacket.Profile = p.Profile;

            infoPacket.InitialInventoryMongoId = ((ICoopInventoryController)ItemFinder.GetPlayerInventoryController(p)).GetMongoId();
            return infoPacket;
        }
    }
}
