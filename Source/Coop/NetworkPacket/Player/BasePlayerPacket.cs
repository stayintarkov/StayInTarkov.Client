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

        //protected void WriteHeaderAndProfileId(NetworkWriter writer)
        //{
        //    WriteHeader(writer);
        //    writer.Write(ProfileId);
        //}

        /// <summary>
        /// Auto discover Client Player object and Process on them
        /// </summary>
        public override void Process()
        {
            //StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}:{Method}");

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

            if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
            {
                Process(client);
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