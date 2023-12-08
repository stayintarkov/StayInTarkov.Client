using Comfort.Common;
using EFT;
using EFT.Weather;
using StayInTarkov.Coop.Matchmaker;
using System;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.LocalGame
{
    public class LocalGame_Weather_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.LocalGame);

            var method = ReflectionHelpers.GetAllMethodsForType(t)
                .LastOrDefault(x => x.GetParameters().Length == 1
                && x.GetParameters()[0].Name.Contains("timeAndWeather")
                );
            return method;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref TimeAndWeatherSettings timeAndWeather)
        {
            Logger.LogDebug("LocalGame_Weather_Patch:PatchPrefix");

            ///
            /// /*
            /// public static GameDateTime smethod_0(DateTime realDateTime, ref string gameDate, ref string gameTime, ref float timeFactor, bool debug = false)
            /// */
            ///
            ///
            Singleton<GameWorld>.Instance.GameDateTime = (GameDateTime)ReflectionHelpers.GetAllMethodsForType(typeof(GameDateTime), false)
                .First(
                    x =>
                    x.GetParameters().Length == 5
                    && x.GetParameters()[0].Name == "realDateTime"
                    && x.GetParameters()[1].Name == "gameDate"
                    && x.GetParameters()[2].Name == "gameTime"
                ).Invoke(null, new object[] { DateTime.Now, "2023-05-29", "09:00", 1f, true });

            if (WeatherController.Instance != null)
            {
                TOD_Sky.Instance.Components.Time.GameDateTime = Singleton<GameWorld>.Instance.GameDateTime;
                WeatherClass[] randomWeatherNodes = WeatherClass.GetRandomTestWeatherNodes(600, 12);
                long time = randomWeatherNodes[0].Time;
                randomWeatherNodes[0] = new WeatherClass() { };
                randomWeatherNodes[0].Time = time;
                ReflectionHelpers.GetMethodForType(typeof(WeatherController), "method_0").Invoke(WeatherController.Instance, new object[] { randomWeatherNodes });
            }

            if (MatchmakerAcceptPatches.IsClient)
            {

            }
            else
            {
                LocalGameStartingPatch.TimeAndWeather = timeAndWeather;
            }

            return true;
        }
    }
}
