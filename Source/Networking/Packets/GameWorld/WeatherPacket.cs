using LiteNetLib.Utils;

namespace StayInTarkov.Networking.Packets
{
    public struct WeatherPacket(bool isRequest, float cloudDensity, float fog, float lightningThunderProbability, float rain, float temperature, float windX, float windY, float topWindX, float topWindY) : INetSerializable
    {
        public bool IsRequest { get; set; } = isRequest;
        public float CloudDensity { get; set; } = cloudDensity;
        public float Fog { get; set; } = fog;
        public float LightningThunderProbability { get; set; } = lightningThunderProbability;
        public float Rain { get; set; } = rain;
        public float Temperature { get; set; } = temperature;
        public float WindX { get; set; } = windX;
        public float WindY { get; set; } = windY;
        public float TopWindX { get; set; } = topWindX;
        public float TopWindY { get; set; } = topWindY;

        public void Deserialize(NetDataReader reader)
        {
            IsRequest = reader.GetBool();
            CloudDensity = reader.GetFloat();
            Fog = reader.GetFloat();
            LightningThunderProbability = reader.GetFloat();
            Rain = reader.GetFloat();
            Temperature = reader.GetFloat();
            WindX = reader.GetFloat();
            WindY = reader.GetFloat();
            TopWindX = reader.GetFloat();
            TopWindY = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsRequest);
            writer.Put(CloudDensity);
            writer.Put(Fog);
            writer.Put(LightningThunderProbability);                
            writer.Put(Rain);
            writer.Put(Temperature);
            writer.Put(WindX);
            writer.Put(WindY);
            writer.Put(TopWindX);
            writer.Put(TopWindY);
        }
    }
}
