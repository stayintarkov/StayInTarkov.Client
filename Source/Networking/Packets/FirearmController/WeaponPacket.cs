using EFT.InventoryLogic;
using LiteNetLib.Utils;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
*/

namespace StayInTarkov.Networking.Packets
{
    public struct WeaponPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool IsTriggerPressed { get; set; }
        public bool ChangeFireMode { get; set; }
        public Weapon.EFireMode FireMode { get; set; }
        public bool ToggleAim { get; set; }
        public int AimingIndex { get; set; }
        public bool ExamineWeapon { get; set; }
        public bool CheckAmmo { get; set; }
        public bool CheckChamber { get; set; }
        public bool CheckFireMode { get; set; }
        public bool ToggleTacticalCombo { get; set; }
        public SITSerialization.LightStatesPacket LightStatesPacket { get; set; }
        public bool ChangeSightMode { get; set; }
        public SITSerialization.ScopeStatesPacket ScopeStatesPacket { get; set; }
        public bool ToggleLauncher { get; set; }
        public EGesture Gesture { get; set; }
        public bool EnableInventory { get; set; }
        public bool InventoryStatus { get; set; }
        public bool Loot { get; set; }
        public bool Pickup { get; set; }
        public SITSerialization.ReloadMagPacket ReloadMag { get; set; }
        public SITSerialization.QuickReloadMagPacket QuickReloadMag { get; set; }
        public SITSerialization.ReloadWithAmmoPacket ReloadWithAmmo { get; set; }
        public SITSerialization.CylinderMagPacket CylinderMag { get; set; }
        public SITSerialization.ReloadLauncherPacket ReloadLauncher { get; set; }
        public SITSerialization.ReloadBarrelsPacket ReloadBarrels { get; set; }

        public WeaponPacket(string profileId)
        {
            ProfileId = profileId;
            IsTriggerPressed = false;
            ChangeFireMode = false;
            ToggleAim = false;
            ExamineWeapon = false;
            CheckAmmo = false;
            CheckChamber = false;
            CheckFireMode = false;
            ToggleTacticalCombo = false;
            ChangeSightMode = false;
            ToggleLauncher = false;
            Gesture = EGesture.None;
            EnableInventory = false;
            InventoryStatus = false;
            Loot = false;
            Pickup = false;
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            IsTriggerPressed = reader.GetBool();
            ChangeFireMode = reader.GetBool();
            FireMode = (Weapon.EFireMode)reader.GetInt();
            ToggleAim = reader.GetBool();
            if (ToggleAim)
                AimingIndex = reader.GetInt();
            ExamineWeapon = reader.GetBool();
            CheckAmmo = reader.GetBool();
            CheckChamber = reader.GetBool();
            CheckFireMode = reader.GetBool();
            ToggleTacticalCombo = reader.GetBool();
            if (ToggleTacticalCombo)
                LightStatesPacket = SITSerialization.LightStatesPacket.Deserialize(reader);
            ChangeSightMode = reader.GetBool();
            if (ChangeSightMode)
                ScopeStatesPacket = SITSerialization.ScopeStatesPacket.Deserialize(reader);
            ToggleLauncher = reader.GetBool();
            Gesture = (EGesture)reader.GetInt();
            EnableInventory = reader.GetBool();
            InventoryStatus = reader.GetBool();
            Loot = reader.GetBool();
            Pickup = reader.GetBool();
            ReloadMag = SITSerialization.ReloadMagPacket.Deserialize(reader);
            QuickReloadMag = SITSerialization.QuickReloadMagPacket.Deserialize(reader);
            ReloadWithAmmo = SITSerialization.ReloadWithAmmoPacket.Deserialize(reader);
            CylinderMag = SITSerialization.CylinderMagPacket.Deserialize(reader);
            ReloadLauncher = SITSerialization.ReloadLauncherPacket.Deserialize(reader);
            ReloadBarrels = SITSerialization.ReloadBarrelsPacket.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(IsTriggerPressed);
            writer.Put(ChangeFireMode);
            writer.Put((int)FireMode);
            writer.Put(ToggleAim);
            if (ToggleAim)
                writer.Put(AimingIndex);
            writer.Put(ExamineWeapon);
            writer.Put(CheckAmmo);
            writer.Put(CheckChamber);
            writer.Put(CheckFireMode);
            writer.Put(ToggleTacticalCombo);
            if (ToggleTacticalCombo)
                SITSerialization.LightStatesPacket.Serialize(writer, LightStatesPacket);
            writer.Put(ChangeSightMode);
            if (ChangeSightMode)
                SITSerialization.ScopeStatesPacket.Serialize(writer, ScopeStatesPacket);
            writer.Put(ToggleLauncher);
            writer.Put((int)Gesture);
            writer.Put(EnableInventory);
            writer.Put(InventoryStatus);
            writer.Put(Loot);
            writer.Put(Pickup);
            SITSerialization.ReloadMagPacket.Serialize(writer, ReloadMag);
            SITSerialization.QuickReloadMagPacket.Serialize(writer, QuickReloadMag);
            SITSerialization.ReloadWithAmmoPacket.Serialize(writer, ReloadWithAmmo);
            SITSerialization.CylinderMagPacket.Serialize(writer, CylinderMag);
            SITSerialization.ReloadLauncherPacket.Serialize(writer, ReloadLauncher);
            SITSerialization.ReloadBarrelsPacket.Serialize(writer, ReloadBarrels);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
