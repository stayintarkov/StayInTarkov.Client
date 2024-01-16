using EFT.InventoryLogic;
using LiteNetLib.Utils;
using StayInTarkov.Networking;
using static StayInTarkov.Networking.SITSerialization;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
*/

namespace StayInTarkov.Coop.NetworkPacket.FirearmController
{
    public struct WeaponPacket : INetSerializable
    {
        public bool ShouldSend { get; private set; } = false;
        public string ProfileId { get; set; }
        public bool HasMalfunction { get; set; }
        public Weapon.EMalfunctionState MalfunctionState { get; set; }
        public bool IsTriggerPressed { get; set; }
        public bool HasShotInfo { get; set; }
        public ShotInfoPacket ShotInfoPacket { get; set; }
        public bool ChangeFireMode { get; set; }
        public Weapon.EFireMode FireMode { get; set; }
        public bool ToggleAim { get; set; }
        public int AimingIndex { get; set; }
        public bool ExamineWeapon { get; set; }
        public bool CheckAmmo { get; set; }
        public bool CheckChamber { get; set; }
        public bool CheckFireMode { get; set; }
        public bool ToggleTacticalCombo { get; set; }
        public LightStatesPacket LightStatesPacket { get; set; }
        public bool ChangeSightMode { get; set; }
        public ScopeStatesPacket ScopeStatesPacket { get; set; }
        public bool ToggleLauncher { get; set; }
        public EGesture Gesture { get; set; }
        public bool EnableInventory { get; set; }
        public bool InventoryStatus { get; set; }
        public bool Loot { get; set; }
        public bool Pickup { get; set; }
        public bool HasReloadMagPacket { get; set; }
        public ReloadMagPacket ReloadMagPacket { get; set; }
        public bool HasQuickReloadMagPacket { get; set; }
        public QuickReloadMagPacket QuickReloadMag { get; set; }
        public bool HasReloadWithAmmoPacket { get; set; }
        public SITSerialization.ReloadWithAmmoPacket ReloadWithAmmo { get; set; }
        public bool HasCylinderMagPacket { get; set; }
        public CylinderMagPacket CylinderMag { get; set; }
        public bool HasReloadLauncherPacket { get; set; }
        public ReloadLauncherPacket ReloadLauncher { get; set; }
        public bool HasReloadBarrelsPacket { get; set; }
        public SITSerialization.ReloadBarrelsPacket ReloadBarrels { get; set; }
        public bool HasGrenadePacket { get; set; }
        public SITSerialization.GrenadePacket GrenadePacket { get; set; }
        public bool HasCompassChange { get; set; }
        public bool CompassState { get; set; }

        public WeaponPacket(string profileId)
        {
            ProfileId = profileId;
            HasMalfunction = false;
            IsTriggerPressed = false;
            HasShotInfo = false;
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
            HasReloadMagPacket = false;
            HasQuickReloadMagPacket = false;
            HasReloadWithAmmoPacket = false;
            HasCylinderMagPacket = false;
            HasReloadLauncherPacket = false;
            HasReloadBarrelsPacket = false;
            HasGrenadePacket = false;
            HasCompassChange = false;
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            HasMalfunction = reader.GetBool();
            if (HasMalfunction)
                MalfunctionState = (Weapon.EMalfunctionState)reader.GetInt();
            IsTriggerPressed = reader.GetBool();
            HasShotInfo = reader.GetBool();
            if (HasShotInfo)
                ShotInfoPacket = ShotInfoPacket.Deserialize(reader);
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
                LightStatesPacket = LightStatesPacket.Deserialize(reader);
            ChangeSightMode = reader.GetBool();
            if (ChangeSightMode)
                ScopeStatesPacket = ScopeStatesPacket.Deserialize(reader);
            ToggleLauncher = reader.GetBool();
            Gesture = (EGesture)reader.GetInt();
            EnableInventory = reader.GetBool();
            InventoryStatus = reader.GetBool();
            Loot = reader.GetBool();
            Pickup = reader.GetBool();
            HasReloadMagPacket = reader.GetBool();
            if (HasReloadMagPacket)
                ReloadMagPacket = ReloadMagPacket.Deserialize(reader);
            HasQuickReloadMagPacket = reader.GetBool();
            if (HasQuickReloadMagPacket)
                ReloadMagPacket.Deserialize(reader);
            HasReloadWithAmmoPacket = reader.GetBool();
            if (HasReloadWithAmmoPacket)
                ReloadWithAmmo = SITSerialization.ReloadWithAmmoPacket.Deserialize(reader);
            HasCylinderMagPacket = reader.GetBool();
            if (HasCylinderMagPacket)
                CylinderMag = CylinderMagPacket.Deserialize(reader);
            HasReloadLauncherPacket = reader.GetBool();
            if (HasReloadLauncherPacket)
                ReloadLauncher = ReloadLauncherPacket.Deserialize(reader);
            HasReloadBarrelsPacket = reader.GetBool();
            if (HasReloadBarrelsPacket)
                ReloadBarrels = SITSerialization.ReloadBarrelsPacket.Deserialize(reader);
            HasGrenadePacket = reader.GetBool();
            if (HasGrenadePacket)
                GrenadePacket = SITSerialization.GrenadePacket.Deserialize(reader);
            HasCompassChange = reader.GetBool();
            if (HasCompassChange)
                CompassState = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(HasMalfunction);
            if (HasMalfunction)
                writer.Put((int)MalfunctionState);
            writer.Put(IsTriggerPressed);
            writer.Put(HasShotInfo);
            if (HasShotInfo)
                ShotInfoPacket.Serialize(writer, ShotInfoPacket);
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
                LightStatesPacket.Serialize(writer, LightStatesPacket);
            writer.Put(ChangeSightMode);
            if (ChangeSightMode)
                ScopeStatesPacket.Serialize(writer, ScopeStatesPacket);
            writer.Put(ToggleLauncher);
            writer.Put((int)Gesture);
            writer.Put(EnableInventory);
            writer.Put(InventoryStatus);
            writer.Put(Loot);
            writer.Put(Pickup);
            writer.Put(HasReloadMagPacket);
            if (HasReloadMagPacket)
                ReloadMagPacket.Serialize(writer, ReloadMagPacket);
            writer.Put(HasQuickReloadMagPacket);
            if (HasQuickReloadMagPacket)
                QuickReloadMagPacket.Serialize(writer, QuickReloadMag);
            writer.Put(HasReloadWithAmmoPacket);
            if (HasReloadWithAmmoPacket)
                SITSerialization.ReloadWithAmmoPacket.Serialize(writer, ReloadWithAmmo);
            writer.Put(HasCylinderMagPacket);
            if (HasCylinderMagPacket)
                CylinderMagPacket.Serialize(writer, CylinderMag);
            writer.Put(HasReloadLauncherPacket);
            if (HasReloadLauncherPacket)
                ReloadLauncherPacket.Serialize(writer, ReloadLauncher);
            writer.Put(HasReloadBarrelsPacket);
            if (HasReloadBarrelsPacket)
                SITSerialization.ReloadBarrelsPacket.Serialize(writer, ReloadBarrels);
            writer.Put(HasGrenadePacket);
            if (HasGrenadePacket)
                SITSerialization.GrenadePacket.Serialize(writer, GrenadePacket);
            writer.Put(HasCompassChange);
            if (HasCompassChange)
                writer.Put(CompassState);
        }

        public void ToggleSend()
        {
            if (!ShouldSend)
                ShouldSend = true;
        }
    }
}
