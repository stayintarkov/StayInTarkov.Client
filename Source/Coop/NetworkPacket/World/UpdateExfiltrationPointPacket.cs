using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StayInTarkov.Coop.NetworkPacket.World
{
    /// <summary>
    /// Sent when an exfil point changes status. Contains information needed to synchronize countdown exfils
    /// </summary>
    public sealed class UpdateExfiltrationPointPacket : BasePacket
    {
        public string PointName;
        public EFT.Interactive.EExfiltrationStatus Command;
        public List<string> QueuedPlayers;

        public UpdateExfiltrationPointPacket() : base(nameof(UpdateExfiltrationPointPacket)) {}

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(PointName);
            writer.Write((byte)Command);
            writer.Write((byte)QueuedPlayers.Count);
            foreach (var p in QueuedPlayers)
            {
                writer.Write(p);
            }
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            PointName = reader.ReadString();
            Command = (EFT.Interactive.EExfiltrationStatus)reader.ReadByte();
            byte count = reader.ReadByte();
            QueuedPlayers = new(count);
            for (int i = 0; i < count; i++)
            {
                QueuedPlayers.Add(reader.ReadString());
            }
            return this;
        }
        public override void Process()
        {
            var point = ExfiltrationControllerClass.Instance.ExfiltrationPoints.First(x => x.Settings.Name == PointName);
            if (point == null)
            {
                return;
            }

            ExfiltrationControllerClass.Instance.UpdatePoint(PointName, Command, QueuedPlayers);
        }
    }
}
