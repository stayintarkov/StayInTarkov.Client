using StayInTarkov.AkiSupport.Singleplayer.Patches.ScavMode;
using EFT;
using System;
using StayInTarkov.AkiSupport.Singleplayer.Models.ScavMode;

namespace StayInTarkov.AkiSupport.Singleplayer.Utils.InRaid
{
    /// <summary>
    /// Allow modders to access changes made to the current (or most recent) raid
    /// </summary>
    public static class RaidChangesUtil
    {
        /// <summary>
        /// The UTC time when raid changes were last applied
        /// </summary>
        public static DateTime RaidChangesAppliedUtcTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// If raid changes have been completed
        /// </summary>
        public static bool HaveChangesBeenApplied => RaidChangesAppliedUtcTime != DateTime.MinValue;

        /// <summary>
        /// If the most recent raid was a Scav run
        /// </summary>
        public static bool IsScavRaid { get; private set; } = false;

        /// <summary>
        /// The location ID of the map for the current (or most recent) raid
        /// </summary>
        public static string LocationId { get; private set; } = string.Empty;

        /// <summary>
        /// The original escape time for the current (or most recent) raid, in minutes
        /// </summary>
        public static int OriginalEscapeTimeMinutes { get; private set; } = int.MaxValue;

        /// <summary>
        /// The original escape time for the current (or most recent) raid, in seconds
        /// </summary>
        public static int OriginalEscapeTimeSeconds => OriginalEscapeTimeMinutes * 60;

        /// <summary>
        /// The updated escape time for the current (or most recent) raid, in minutes
        /// </summary>
        public static int NewEscapeTimeMinutes { get; private set; } = int.MaxValue;

        /// <summary>
        /// The updated escape time for the current (or most recent) raid, in seconds
        /// </summary>
        public static int NewEscapeTimeSeconds => NewEscapeTimeMinutes * 60;

        /// <summary>
        /// The reduction in the escape time for the current (or most recent) raid, in minutes
        /// </summary>
        public static int RaidTimeReductionMinutes => OriginalEscapeTimeMinutes - NewEscapeTimeMinutes;

        /// <summary>
        /// The reduction in the escape time for the current (or most recent) raid, in seconds
        /// </summary>
        public static int RaidTimeReductionSeconds => RaidTimeReductionMinutes * 60;

        /// <summary>
        /// The fraction of raid time that will be remaining when you spawn into the map
        /// </summary>
        public static float RaidTimeRemainingFraction => (float)NewEscapeTimeMinutes / OriginalEscapeTimeMinutes;

        /// <summary>
        /// The original minimum time (in seconds) you must stay in the raid to get a "Survived" status (unless your XP is high enough) for the current (or most recent) raid
        /// </summary>
        public static int OriginalSurvivalTimeSeconds { get; private set; } = int.MaxValue;

        /// <summary>
        /// The updated minimum time (in seconds) you must stay in the raid to get a "Survived" status (unless your XP is high enough) for the current (or most recent) raid
        /// </summary>
        public static int NewSurvivalTimeSeconds { get; private set; } = int.MaxValue;

        /// <summary>
        /// The reduction in the minimum time you must stay in the raid to get a "Survived" status (unless your XP is high enough) for the current (or most recent) raid 
        /// </summary>
        public static int SurvivalTimeReductionSeconds => OriginalSurvivalTimeSeconds - NewSurvivalTimeSeconds;

        /// <summary>
        /// Update the changes that will be made for the raid. This should be called just before applying changes. 
        /// </summary>
        /// <param name="raidSettings">The raid settings for the raid that will be altered</param>
        /// <param name="raidChanges">The changes that will be made to the raid</param>
        internal static void UpdateRaidChanges(RaidSettings raidSettings, RaidTimeResponse raidChanges)
        {
            // Reset so HaveChangesBeenApplied=false while changes are being applied
            RaidChangesAppliedUtcTime = DateTime.MinValue;

            IsScavRaid = raidSettings.IsScav;

            LocationId = raidSettings.SelectedLocation.Id;

            OriginalEscapeTimeMinutes = raidSettings.SelectedLocation.EscapeTimeLimit;
            NewEscapeTimeMinutes = raidChanges.RaidTimeMinutes;

            OriginalSurvivalTimeSeconds = raidChanges.OriginalSurvivalTimeSeconds;
            NewSurvivalTimeSeconds = raidChanges.NewSurviveTimeSeconds ?? OriginalSurvivalTimeSeconds;
        }

        /// <summary>
        /// This should be called just after all raid changes have been completed
        /// </summary>
        internal static void ConfirmRaidChanges()
        {
            // This will also set HaveChangesBeenApplied=true
            RaidChangesAppliedUtcTime = DateTime.UtcNow;
        }
    }
}
