using Comfort.Common;
using EFT;
using System;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid
{
    /// <summary>
    /// Allow modders to access information about the current raid time, especially if its reduced for Scav raids
    /// </summary>
    public static class RaidTimeUtil
    {
        /// <summary>
        /// Determines if a raid has loaded by checking if a GameTimer instance has been created for one
        /// </summary>
        public static bool HasRaidLoaded()
        {
            return Singleton<AbstractGame>.Instance?.GameTimer != null;
        }

        /// <summary>
        /// Determines if a raid is in-progress by checking if the GameTimer has started
        /// </summary>
        public static bool HasRaidStarted()
        {
            return Singleton<AbstractGame>.Instance.GameTimer.Started();
        }

        /// <summary>
        /// <para>Calculates the seconds remaining in the current raid.</para>
        /// <para>Please ensure <see cref="HasRaidLoaded"/> is <c>true</c>, or this will throw an exception.</para>
        /// </summary>
        /// <returns>Seconds remaining in the raid</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is no raid in progress</exception>
        public static float GetRemainingRaidSeconds()
        {
            if (!HasRaidLoaded())
            {
                throw new InvalidOperationException("The raid-time remaining can only be calculated when a raid is in-progress");
            }

            if (!HasRaidStarted())
            {
                return RaidChangesUtil.NewEscapeTimeSeconds;
            }

            float remainingTimeSeconds = Singleton<AbstractGame>.Instance.GameTimer.EscapeTimeSeconds();

            // Until the raid starts, remainingTimeSeconds is TimeSpan.MaxValue, so it needs to be reduced to the actual starting raid time
            return Math.Min(remainingTimeSeconds, RaidChangesUtil.NewEscapeTimeSeconds);
        }

        /// <summary>
        /// <para>Calculates the fraction of raid-time remaining relative to the original escape time for the map. 
        /// 1.0 = the raid just started, and 0.0 = the raid is over (and you're MIA).</para>
        /// <para>Please ensure <see cref="HasRaidLoaded"/> is <c>true</c>, or this will throw an exception.</para>
        /// </summary>
        /// <returns>The fraction of raid-time remaining (0.0 - 1.0) relative to the original escape time for the map</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is no raid in progress</exception>
        public static float GetRaidTimeRemainingFraction()
        {
            return GetRemainingRaidSeconds() / RaidChangesUtil.OriginalEscapeTimeSeconds;
        }

        /// <summary>
        /// Calculates the seconds since the player spawned into the raid
        /// </summary>
        /// <returns>Seconds since the player spawned into the raid</returns>
        public static float GetSecondsSinceSpawning()
        {
            return Singleton<AbstractGame>.Instance.GameTimer.PastTimeSeconds();
        }

        /// <summary>
        /// Calculates the elapsed seconds in the raid from the original escape time for the map.
        /// </summary>
        /// <returns>Elapsed seconds in the raid</returns>
        public static float GetElapsedRaidSeconds()
        {
            return RaidChangesUtil.RaidTimeReductionSeconds + GetSecondsSinceSpawning();
        }
    }
}
