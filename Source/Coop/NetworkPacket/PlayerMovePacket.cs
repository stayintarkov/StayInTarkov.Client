using System;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerMovePacket : BasePlayerPacket, IDisposable
    {
        public float pX { get; set; }

        public float pY { get; set; }

        public float pZ { get; set; }

        public float dX { get; set; }
        public float dY { get; set; }
        public float spd { get; set; }

        public PlayerMovePacket(string profileId, float px, float py, float pz, float dx, float dy, float speed) : base(profileId, "Move")
        {
            pX = px;
            pY = py;
            pZ = pz;
            dX = dx;
            dY = dy;
            spd = speed;
        }

        public void Dispose()
        {
            ProfileId = null;
            //StayInTarkovHelperConstants.Logger.LogDebug("PlayerMovePacket.Dispose");
        }
    }
}