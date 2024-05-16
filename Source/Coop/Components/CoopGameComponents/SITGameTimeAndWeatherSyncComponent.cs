using Comfort.Common;
using EFT;
using EFT.Weather;
using StayInTarkov.Coop.NetworkPacket.Raid;
using StayInTarkov.Coop.SITGameModes;
using System;
using UnityEngine.Networking;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    public sealed class SITGameTimeAndWeatherSyncComponent : NetworkBehaviour
    {
        private DateTime LastTimeSent = DateTime.MinValue;

        private BepInEx.Logging.ManualLogSource Logger { get; set; }

        void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITGameTimeAndWeatherSyncComponent));
            Logger.LogDebug($"{nameof(SITGameTimeAndWeatherSyncComponent)}:{nameof(Awake)}");
        }

        void Start()
        {
            Logger.LogDebug($"{nameof(SITGameTimeAndWeatherSyncComponent)}:{nameof(Start)}");
        }


        void Update()
        {
            if ((DateTime.Now - LastTimeSent).Seconds > 15)
            {
                LastTimeSent = DateTime.Now;

                TimeAndWeatherPacket packet = new();

                var sitGame = Singleton<ISITGame>.Instance;

                if (sitGame.GameDateTime != null)
                    packet.GameDateTime = sitGame.GameDateTime.Calculate().Ticks;

                var weatherController = WeatherController.Instance;
                if (weatherController != null)
                {
                    if (weatherController.CloudsController != null)
                        packet.CloudDensity = weatherController.CloudsController.Density;

                    var weatherCurve = weatherController.WeatherCurve;
                    if (weatherCurve != null)
                    {
                        packet.Fog = weatherCurve.Fog;
                        packet.LightningThunderProbability = weatherCurve.LightningThunderProbability;
                        packet.Rain = weatherCurve.Rain;
                        packet.Temperature = weatherCurve.Temperature;
                        packet.Wind = weatherCurve.Wind;
                        packet.TopWind = weatherCurve.TopWind;
                    }

                    Networking.GameClient.SendData(packet.Serialize());
                }
            }
        }
    }
}
