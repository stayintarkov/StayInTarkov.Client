using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using EFT;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    public sealed class TimeAndWeatherPacket : BasePacket
    {
        public TimeAndWeatherPacket() : base(nameof(TimeAndWeatherPacket))
        {
        }

        public float GameDateTime { get; set; }
        public float CloudDensity { get; set; }
        public float Fog { get; set; }
        public float LightningThunderProbability { get; set; }
        public float Rain { get; set; }
        public float Temperature { get; set; }
        public Vector2 Wind { get; set; }
        public Vector2 TopWind { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            //writer.Write(GameDateTime);
            writer.Write(CloudDensity);
            writer.Write(Fog);
            writer.Write(LightningThunderProbability);
            writer.Write(Rain);
            writer.Write(Temperature);
            SITSerialization.Vector2Utils.Serialize(writer, Wind);
            SITSerialization.Vector2Utils.Serialize(writer, TopWind);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            //GameDateTime = reader.ReadSingle();
            CloudDensity = reader.ReadSingle();
            Fog = reader.ReadSingle();
            LightningThunderProbability = reader.ReadSingle();
            Rain = reader.ReadSingle();
            Temperature = reader.ReadSingle();
            Wind = SITSerialization.Vector2Utils.Deserialize(reader);
            TopWind = SITSerialization.Vector2Utils.Deserialize(reader);
            return this;
        }

        public override void Process()
        {
            ReplicateTimeAndWeather();
        }

        void ReplicateTimeAndWeather()
        {
            SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
                return;

            if (!SITMatchmaking.IsClient)
                return;

#if DEBUG
            StayInTarkov.StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(ReplicateTimeAndWeather)}");

#endif

            var gameDateTime = new DateTime(long.Parse(GameDateTime.ToString()));
            if (coopGameComponent.LocalGameInstance is CoopSITGame coopGame && coopGame.GameDateTime != null)
                coopGame.GameDateTime.Reset(gameDateTime);

            var weatherController = EFT.Weather.WeatherController.Instance;
            if (weatherController != null)
            {
                var weatherDebug = weatherController.WeatherDebug;
                if (weatherDebug != null)
                {
                    weatherDebug.Enabled = true;

                    weatherDebug.CloudDensity = float.Parse(this.CloudDensity.ToString());
                    weatherDebug.Fog = float.Parse(this.Fog.ToString());
                    weatherDebug.LightningThunderProbability = float.Parse(this.LightningThunderProbability.ToString());
                    weatherDebug.Rain = float.Parse(this.Rain.ToString());
                    weatherDebug.Temperature = float.Parse(this.Temperature.ToString());
                    weatherDebug.TopWindDirection = this.TopWind;

                    Vector2 windDirection = this.Wind;

                    // working dog sh*t, if you are the programmer, DON'T EVER DO THIS! - dounai2333
                    static bool BothPositive(float f1, float f2) => f1 > 0 && f2 > 0;
                    static bool BothNegative(float f1, float f2) => f1 < 0 && f2 < 0;
                    static bool VectorIsSameQuadrant(Vector2 v1, Vector2 v2, out int flag)
                    {
                        flag = 0;
                        if (v1.x != 0 && v1.y != 0 && v2.x != 0 && v2.y != 0)
                        {
                            if ((BothPositive(v1.x, v2.x) && BothPositive(v1.y, v2.y))
                            || (BothNegative(v1.x, v2.x) && BothNegative(v1.y, v2.y))
                            || (BothPositive(v1.x, v2.x) && BothNegative(v1.y, v2.y))
                            || (BothNegative(v1.x, v2.x) && BothPositive(v1.y, v2.y)))
                            {
                                flag = 1;
                                return true;
                            }
                        }
                        else
                        {
                            if (v1.x != 0 && v2.x != 0)
                            {
                                if (BothPositive(v1.x, v2.x) || BothNegative(v1.x, v2.x))
                                {
                                    flag = 1;
                                    return true;
                                }
                            }
                            else if (v1.y != 0 && v2.y != 0)
                            {
                                if (BothPositive(v1.y, v2.y) || BothNegative(v1.y, v2.y))
                                {
                                    flag = 2;
                                    return true;
                                }
                            }
                        }
                        return false;
                    }

                    for (int i = 1; i < WeatherClass.WindDirections.Count(); i++)
                    {
                        Vector2 direction = WeatherClass.WindDirections[i];
                        if (VectorIsSameQuadrant(windDirection, direction, out int flag))
                        {
                            weatherDebug.WindDirection = (EFT.Weather.WeatherDebug.Direction)i;
                            weatherDebug.WindMagnitude = flag switch
                            {
                                1 => windDirection.x / direction.x,
                                2 => windDirection.y / direction.y,
                                _ => weatherDebug.WindMagnitude
                            };
                            break;
                        }
                    }
                }
                else
                {
                    StayInTarkov.StayInTarkovHelperConstants.Logger.LogError("TimeAndWeather: WeatherDebug is null!");
                }
            }
            else
            {
                StayInTarkov.StayInTarkovHelperConstants.Logger.LogError("TimeAndWeather: WeatherController is null!");
            }
        }
    }
}
