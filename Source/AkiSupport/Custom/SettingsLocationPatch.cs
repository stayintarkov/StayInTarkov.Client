using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace StayInTarkov.AkiSupport.Custom
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Patches/SettingsLocationPatch.cs
    /// Modified by: dounai2333: Applying patch seems make game stop loading forever, I don't really know why, use reflection instead.
    /// </summary>
    public class SettingsLocationPatch
    {
        private static string _sptPath = Path.Combine(Environment.CurrentDirectory, "user", "sptSettings");

        public static void Enable()
        {
            if (!Directory.Exists(_sptPath))
                Directory.CreateDirectory(_sptPath);

            // Screenshot
            FieldInfo DocumentsSettings = ReflectionHelpers.GetFieldFromType(typeof(SettingsManager), "string_0");

            // Game Settings
            FieldInfo ApplicationDataSettings = ReflectionHelpers.GetFieldFromType(typeof(SettingsManager), "string_1");

            // They are 'static' variables, not needed to give a instance.
            DocumentsSettings.SetValue(null, _sptPath);
            ApplicationDataSettings.SetValue(null, _sptPath);
        }
    }
}