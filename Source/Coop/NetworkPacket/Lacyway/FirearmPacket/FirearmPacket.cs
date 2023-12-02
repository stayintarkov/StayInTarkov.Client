using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct FirearmPacket
    {
        public FireModePacket ChangeFireMode;
        public bool ChangeCalibrationPoint;
        public bool ToggleAim;
        public int AimingIndex;
        public bool ExamineWeapon;
        public bool CheckAmmo;
        public bool CheckChamber;
        public bool CheckFireMode;
        public bool ToggleLauncher;
        public bool ReloadBoltAction;
        public ToggleTacticalCombo ToggleTacticalCombo;
        public ChangeSightsMode ChangeSightsMode;
        public LauncherRangeStatePacket LauncherRangeStatePacket;
        public FiredShotInfo? FiredShotInfos;
        public FlareShotInfo FlareShotInfo;
        public LauncherReloadInfo LauncherReloadInfo;
        public EnableInventoryPacket EnableInventoryPacket;
        public HideWeaponPacket HideWeaponPacket;
        public ReloadMagPacket1 ReloadMagPacket;
        public QuickReloadMag QuickReloadMagPacket;
        public ReloadWithAmmoPacket ReloadWithAmmoPacket;
        public ReloadBarrelsPacket ReloadBarrelsPacket;
        public GStruct301<PrevShot> ShotsForApprovement;
        public CompassPacket CompassPacket;
        public RadioTransmitterPacket RadioTransmitterPacket;
        public LighthouseTraderZoneDataPacket LighthouseTraderZoneDataPacket;
        public CylinderMagStatusPacket CylinderMagStatusPacket;
        public RollCylinderPacket RollCylinderPacket;
        public bool IsInImportantState;
        public EGesture Gesture;
        public float TimeStamp;
    }
}
