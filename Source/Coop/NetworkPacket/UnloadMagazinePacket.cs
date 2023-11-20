namespace StayInTarkov.Coop.NetworkPacket
{
    public class UnloadMagazinePacket : BasePlayerPacket
    {
        public string MagazineId { get; set; }
        public string MagazineTemplateId { get; set; }

        public UnloadMagazinePacket(
            string profileId
            , string magazineId
            , string magazineTemplateId
            )
            : base(profileId, "PlayerInventoryController_UnloadMagazine")
        {
            this.MagazineId = magazineId;
            this.MagazineTemplateId = magazineTemplateId;
        }
    }
}
