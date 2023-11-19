using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov
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
