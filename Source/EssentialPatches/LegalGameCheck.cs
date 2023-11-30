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

        public static bool LegalGameFound { get; private set; } = false;

        public static bool LegalityCheck(BepInEx.Configuration.ConfigFile config)
        {
            if (Checked || LegalGameFound)
                return LegalGameFound;

            try
            {
                var gamefilePath = RegistryManager.GamePathEXE;
                if (LC1A(gamefilePath))
                {
                    if (LC2B(gamefilePath))
                    {
                        if (LC3C(gamefilePath))
                        {
                            StayInTarkovHelperConstants.Logger.LogInfo("Legal Game Found. Thanks for supporting BSG!");
                            Checked = true;
                            LegalGameFound = true;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StayInTarkovHelperConstants.Logger.LogError(ex.ToString());
            }

            Checked = true;
            LegalGameFound = false;
            StayInTarkovHelperConstants.Logger.LogError(StayInTarkovPlugin.IllegalMessage);
            return false;
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
