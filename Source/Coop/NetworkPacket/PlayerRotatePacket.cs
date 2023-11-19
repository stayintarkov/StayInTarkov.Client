using System.Numerics;

namespace SIT.Core.Coop.NetworkPacket
{
    internal class PlayerRotatePacket : BasePlayerPacket
    {
        public float x { get; set; }
        public float y { get; set; }

        public PlayerRotatePacket(Vector2 rotate, string profileId, string method) : base(profileId, method)
        {
            x = rotate.X;
            y = rotate.Y;
        }
    }
}
