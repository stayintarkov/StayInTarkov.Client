using Comfort.Common;
using EFT;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Player;
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
    public sealed class RequestSpawnPlayersPacket : BasePacket
    {
        public string[] ExistingProfileIds { get; set; } = new string[0];

        public RequestSpawnPlayersPacket() : base(nameof(RequestSpawnPlayersPacket))
        {
            //StayInTarkovHelperConstants.Logger.LogInfo("Created RequestSpawnPlayersPacket");
        }

        public RequestSpawnPlayersPacket(in string[] existingProfileIds) : this()
        {
            ExistingProfileIds = existingProfileIds;
        }

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ExistingProfileIds.Length);
            foreach (var profileId in ExistingProfileIds)
            {
                writer.Write(profileId);
            }

            StayInTarkovHelperConstants.Logger.LogInfo("Serialized RequestSpawnPlayersPacket");

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);

            var length = reader.ReadInt32();
            ExistingProfileIds = new string[length];
            for(var i = 0; i < length; i++)
            {
                ExistingProfileIds[i] = reader.ReadString();
            }

            return this;
        }

        public override void Process()
        {

            // ------------------------------------------------------------------------------------------
            // Receive the Packet
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(RequestSpawnPlayersPacket)}:{nameof(Process)}");
            EFT.UI.ConsoleScreen.Log($"{nameof(RequestSpawnPlayersPacket)}:{nameof(Process)}");
            // ------------------------------------------------------------------------------------------

            SpawnPlayersPacket.CreateFromGame(ExistingProfileIds);
        }

    }
}
