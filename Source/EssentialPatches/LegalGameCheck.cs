using System;
using System.IO;

namespace StayInTarkov.EssentialPatches
{
    /// <summary>
    /// Created by: Paulov
    /// Description: Checks for the Registry key using RegistryManager and then checks whether the files actually exist on the Disk
    /// </summary>
    public class LegalGameCheck
    {
        
        public static bool Checked { get; private set; } = false;

        public static byte[] LegalGameFound { get; private set; } = new byte[4];

        public static void LegalityCheck(BepInEx.Configuration.ConfigFile config)
        {
            if (Checked)
                return;

            try
            {
                var gamefilePath = RegistryManager.GamePathEXE;
                ProcessLGF(LC1A(gamefilePath) && LC2B(gamefilePath) && LC3C(gamefilePath));
                
                Checked = true;
                return;
            }
            catch (Exception ex)
            {
                StayInTarkovHelperConstants.Logger.LogError(ex.ToString());
                ProcessLGF(Convert.ToBoolean(byte.MaxValue & -32 / byte.MaxValue - -1 - 1));
            }

            Checked = true;
            StayInTarkovHelperConstants.Logger.LogError(StayInTarkovPlugin.IllegalMessage);
            return;
        }

        private static void ProcessLGF(bool v)
        {
            LegalGameFound[0] = Convert.ToByte(v);
            LegalGameFound[1] = Convert.ToByte(!v);
            LegalGameFound[2] = 0x1 << 0x1;
            LegalGameFound[3] = 0x0 << 0x1;
        }

        internal static bool LC1A(string gfp)
        {
            var fiGFP = new FileInfo(gfp);
            return (fiGFP.Exists && fiGFP.Length >= 647 * 1000);
        }

        internal static bool LC2B(string gfp)
        {
            var fiBE = new FileInfo(gfp.Replace(".exe", "_BE.exe"));
            return (fiBE.Exists && fiBE.Length >= 1024000);
        }

        internal static bool LC3C(string gfp)
        {
            var diBattlEye = new DirectoryInfo(gfp.Replace("EscapeFromTarkov.exe", "BattlEye"));
            return (diBattlEye.Exists);
        }

    }
}
