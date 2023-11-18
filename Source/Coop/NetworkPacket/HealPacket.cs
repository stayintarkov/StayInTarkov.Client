namespace SIT.Core.Coop.NetworkPacket
{
    public class HealPacket : BasePlayerPacket
    {
        public EBodyPart bodyPart { get; set; }
        public float value { get; set; }
        public override string Method { get => "Heal"; }
    }
}
