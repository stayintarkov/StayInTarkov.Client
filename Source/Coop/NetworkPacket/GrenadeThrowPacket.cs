namespace StayInTarkov.Coop.NetworkPacket
{
    public class GrenadeThrowPacket : BasePlayerPacket
    {
        public float rX { get; set; }

        public float rY { get; set; }

        public GrenadeThrowPacket(string profileId, UnityEngine.Vector2 rotation, string method) : base(profileId, method)
        {
            rX = rotation.x;
            rY = rotation.y;
        }
    }
}