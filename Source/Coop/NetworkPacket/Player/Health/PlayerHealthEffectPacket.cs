using StayInTarkov.Coop.Players;
using System.IO;

namespace StayInTarkov.Coop.NetworkPacket.Player.Health
{
    public sealed class PlayerHealthEffectPacket : BasePlayerPacket
    {
        public HealthSyncPacket HealthEffectPacket { get; set; }

        public PlayerHealthEffectPacket() : base("", nameof(PlayerHealthEffectPacket))
        {
        }

        public PlayerHealthEffectPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerHealthEffectPacket))
        {
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            

            return this;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);

            return ms.ToArray();    
        }


        public override void Process()
        {
            if (Method != nameof(PlayerHealthEffectPacket))
                return;

           


        }

        protected override void Process(CoopPlayerClient client)
        {
            base.Process(client);
        }
    }
}
