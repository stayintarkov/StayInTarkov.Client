using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib.Tools;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Multiplayer.BTR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.BTR
{
    public sealed class BTRInteractionPacket : BasePlayerPacket
    {
        public BTRInteractionPacket() : base("", nameof(BTRInteractionPacket))
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BTRInteractionPacket));
        }

        public PlayerInteractPacket InteractPacket;

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            this.WriteHeaderAndProfileId(writer);
            writer.Write(InteractPacket.HasInteraction);
            writer.Write((int)InteractPacket.InteractionType);
            writer.Write(InteractPacket.SideId);
            writer.Write(InteractPacket.SlotId);
            writer.Write(InteractPacket.Fast);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            this.ReadHeaderAndProfileId(reader);

            InteractPacket = new()
            {
                HasInteraction = reader.ReadBoolean(),
                InteractionType = (EInteractionType)reader.ReadInt(),
                SideId = reader.ReadByte(),
                SlotId = reader.ReadByte(),
                Fast = reader.ReadBoolean()
            };
            return this;
        }

        public override void Process()
        {
            Logger.LogDebug($"{nameof(Process)}");
            if (SITMatchmaking.IsServer)
            {
                var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                Singleton<BTRManager>.Instance.PlayerInteractWithDoor(mainPlayer, this.InteractPacket);
            }
            base.Process();
        }

        protected override void Process(CoopPlayerClient client)
        {
            Logger.LogDebug($"{nameof(Process)}(client)");
            Singleton<BTRManager>.Instance.PlayerInteractWithDoor(client, this.InteractPacket);
        }

    }
}
