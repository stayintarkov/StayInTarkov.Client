using Microsoft.Win32;

namespace StayInTarkov.EssentialPatches
{
    public static class RegistryManager
    {
        public static string GamePathEXE
        {
            get
            {

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov"))
                {
                    if (key != null)
                    {
                        string exePath = key.GetValue("DisplayIcon").ToString();
                        return exePath;
                    }
                }

                return string.Empty;
            }
        }

    }
}
