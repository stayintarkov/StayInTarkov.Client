using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Coop.Players;
using System.IO;
using UnityEngine.Networking;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public class BasePlayerPacket : BasePacket
    {
        public static ManualLogSource Logger { get; private set; }

        static BasePlayerPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BasePlayerPacket));
        }

        [JsonProperty(PropertyName = "profileId")]
        public string ProfileId { get; set; }

        public BasePlayerPacket() : base(null)
        {
        }

        public BasePlayerPacket(string profileId, string method) : base(method)
        {
            if (profileId != null)
                ProfileId = new string(profileId.ToCharArray());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ProfileId.Clear();
                ProfileId = null;
            }
            //StayInTarkovHelperConstants.Logger.LogDebug("PlayerMovePacket.Dispose");
        }


        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            return this;
        }

        protected void ReadHeaderAndProfileId(BinaryReader reader)
        {
            ReadHeader(reader);
            ProfileId = reader.ReadString();
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            return ms.ToArray();
        }

        protected void WriteHeaderAndProfileId(BinaryWriter writer)
        {
            WriteHeader(writer);
            writer.Write(ProfileId);
        }

        /// <summary>
        /// Auto discover Client Player object and Process on them
        /// </summary>
        public override void Process()
        {
            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                Logger.LogError("Unable to obtain Game Component");
                return;
            }

            // If it is my Player, it wont be a Client. So ignore.
            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer.ProfileId == ProfileId)
                return;

            // Find the affected Player
            if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
            {
                Process(client);
            }
            // This deals with other mods adding players to the world. TODO: Maybe remove the above call?
            else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.allAlivePlayersByID.ContainsKey(ProfileId))
            {
                if (Singleton<GameWorld>.Instance.allAlivePlayersByID[ProfileId] is CoopPlayerClient client2)
                    Process(client2);
            }
            else
            {
                Logger.LogError($"Unable to find {ProfileId}");
            }
        }

        /// <summary>
        /// Process on Discovered Client object
        /// </summary>
        /// <param name="client">Client entity</param>
        protected virtual void Process(CoopPlayerClient client)
        {

        }

    }
}