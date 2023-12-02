namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    public struct TacticalComboPacket
    {
        public bool ToggleTacticalCombo { get; set; }
        public ScopePacket[] TacticalComboStatuses { get; set; }
    }

    public struct ScopePacket
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public int SelectedMode { get; set; }
        public int ScopeCalibrationIndex { get; set; }
    }
}