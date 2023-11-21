namespace StayInTarkov.Coop.NetworkPacket
{
    public class PlayerProceedPacket : ItemPlayerPacket
    {
        public bool Scheduled { get; set; }

        public PlayerProceedPacket(string profileId, string itemId, string templateId, bool scheduled, string method) : base(profileId, itemId, templateId, method)
        {
            Scheduled = scheduled;
        }
    }
}