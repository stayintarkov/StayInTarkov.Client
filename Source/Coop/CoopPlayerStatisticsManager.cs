using EFT;

namespace StayInTarkov.Coop
{
    public class CoopPlayerStatisticsManager : AStatisticsManagerForPlayer, IStatisticsManager
    {
        public static Profile Profile { get; set; }

        public override void BeginStatisticsSession()
        {
            base.BeginStatisticsSession();

            Profile = base.Profile_0;
        }

        protected override void ShowStatNotification(LocalizationKey localizationKey1, LocalizationKey localizationKey2, int value)
        {

        }
    }
}
