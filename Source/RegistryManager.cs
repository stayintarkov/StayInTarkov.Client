using BepInEx.Logging;
using Microsoft.Win32;
using System.IO;

namespace StayInTarkov
{
    public static class RegistryManager
    {
        public static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource($"{nameof(RegistryManager)}");
        public static string GamePathEXE
        {
            get
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov"))
                {
                    if (key != null)
                    {
                        string installPath = key.GetValue("InstallLocation")?.ToString();

                        if (!string.IsNullOrEmpty(installPath) && File.Exists(Path.Combine(installPath, "EscapeFromTarkov.exe")))
                        {
                            var exePath = Path.Combine(installPath, "EscapeFromTarkov.exe");
                            Logger.LogInfo($"{nameof(RegistryManager)}:Found Exe {exePath} using InstallLocation key");
                            return exePath;
                        }
                    }
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov"))
                {
                    if (key != null)
                    {
                        string exePath = key.GetValue("DisplayIcon")?.ToString();
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            Logger.LogInfo($"{nameof(RegistryManager)}:Found Exe {exePath} using DisplayIcon key");
                            return exePath;
                        }
                    }
                }

                
                return string.Empty;
            }
        }

    }
}
