using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;


namespace StayInTarkov.EssentialPatches
{
    //Made by SlejmUr <slejmur@protonmail.com>, modified for SIT
    //This portion of the code is under MIT License.
    internal class LGC
    {

        public static string IllegalMessage { get; }
            = StayInTarkovPlugin.LanguageDictionaryLoaded && StayInTarkovPlugin.LanguageDictionary.ContainsKey("ILLEGAL_MESSAGE")
            ? StayInTarkovPlugin.LanguageDictionary["ILLEGAL_MESSAGE"]
            : "Illegal game found. Please buy, install and launch the game once.";
        
        //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov
        //InstallLocation
        //DisplayVersion

        public static string GetREGDisplayVersion()
        {
            //Make sure we are on windows. (Sorry for some linux wine users)
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return string.Empty;
            try
            {
                return (string)Registry.GetValue("Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\EscapeFromTarkov", "DisplayVersion", string.Empty);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetInstallLocation()
        {
            //Make sure we are on windows.  (Sorry for some linux wine users)
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return string.Empty;
            try
            {
                return (string)Registry.GetValue("Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\EscapeFromTarkov", "InstallLocation", string.Empty);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool IfDirFromTarkov()
        {
            var loc = GetInstallLocation();
            if (string.IsNullOrEmpty(loc))
                return true;
            string curDir = Directory.GetCurrentDirectory();    //hopefully we get the Dir where we currently are
            return (curDir == loc);
        }

        public static bool RunTarkovLGC(BepInEx.Configuration.ConfigFile config)
        {
            var lcRemover = config.Bind<bool>("Debug Settings", "LC Remover", false).Value;
            if (lcRemover)
                return true;
            if (IfDirFromTarkov())
            {
                //make sure we are not on live, so we quit
                //Application.Quit();
               StayInTarkovHelperConstants.Logger.LogError(IllegalMessage);
               return false;
            }
            //  tbh, checking this is shit.
            //  I rather just let it install on live and always patch before someone start the game,
            //  yes. this include start from original launcher
            var sha = SHA1.Create();
            foreach (var item in sha1_kv)
            {
                var hash = sha.ComputeHash(File.ReadAllBytes(item.Key));
                var hash_string = Convert.ToString(hash).Replace("-", "");
                if (item.Value != hash_string)
                {
                    StayInTarkovHelperConstants.Logger.LogError(IllegalMessage); //CRCErrorMsg?
                    return false;
                }
                //Application.Quit();
            }
            sha.Dispose();
            //StayInTarkovHelperConstants.Logger.LogError("Legal game, have fun!");
            return true;
        }

        static readonly Dictionary<string, string> sha1_kv = new()
        {
            { "EscapeFromTarkov_BE.exe", "8566a2400a9d2f5123f439b30744bb52e169d6dd" },
            { "UnityCrashHandler64.exe", "80cee5a8f66e8f4e6d5d3e2f277d9ec846e89f91" },
            { "Uninstall.exe", "7646c0fc24ae35cd688808da240f27ce58973286" },
            { "EscapeFromTarkov_Data/app.info", "de9a095b95ec4105a06e9bc0f3bba150902818a8" },
            { "UnityPlayer.dll", "b88cecc9aef25b04c0c0c1ad98bfcf0da1c8ed8c" },
            { "NLog/NLog.config", "b4839b8057276da84a59ece081403a73b272674d" }
        };
    }
}
