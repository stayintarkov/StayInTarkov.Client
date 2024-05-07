using Comfort.Common;
using EFT;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class ExtractedPlayerPacket : BasePlayerPacket
    {
        public ExtractedPlayerPacket() : base("", nameof(ExtractedPlayerPacket))
        {
        }

        public ExtractedPlayerPacket(string profileId) : base(profileId, nameof(ExtractedPlayerPacket))
        {
            this.ProfileId = profileId;
        }

        protected override void Process(CoopPlayerClient client)
        {
            StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ExtractedPlayerPacket)}:Process({client.ProfileId})");
            var gameInstance = Singleton<ISITGame>.Instance;
            if (!gameInstance.ExtractedPlayers.Contains(client.ProfileId))
                 gameInstance.ExtractedPlayers.Add(client.ProfileId);
        }
    }
}
