using EFT;

namespace DrakiaXYZ.Waypoints.Helpers
{
    public class PatrolWayCustom : PatrolWay
    {
        // Custom patrol ways will always be suitable
        public override bool Suitable(BotOwner bot, IBotData data)
        {
            return true;
        }
    }
}
