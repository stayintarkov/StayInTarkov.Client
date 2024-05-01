using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.AI
{
    public class DespawnAIPacket : BasePacket
    {
        public string AIProfileId { get; set; }

        public DespawnAIPacket() : base(nameof(DespawnAIPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(AIProfileId);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            AIProfileId = reader.ReadString();
            return this;
        }

        public override void Process()
        {
            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                return;

#if DEBUG
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(DespawnAIPacket)}: {AIProfileId}");
#endif

            if (!coopGameComponent.ProfileIdsAI.Contains(AIProfileId))
                return;

            try
            {
                EFT.Player Bot = coopGameComponent.Players[AIProfileId];
                Bot.Dispose();
                coopGameComponent.ProfileIdsAI.Remove(AIProfileId);
                UnityEngine.Object.DestroyImmediate(Bot);
                UnityEngine.Object.Destroy(Bot);
            }
            catch(Exception ex)
            {
                StayInTarkovHelperConstants.Logger.LogError($"{nameof(DespawnAIPacket)}: {ex}");
            }
        }
    }
}
