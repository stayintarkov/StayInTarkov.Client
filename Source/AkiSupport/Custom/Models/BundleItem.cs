using System.Runtime.Serialization;

namespace Aki.Custom.Models
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Models/BundleItem.cs
    /// </summary>
    [DataContract]
    public struct BundleItem
    {
        [DataMember(Name = "filename")]
        public string FileName;

        [DataMember(Name = "crc")]
        public uint Crc;

        [DataMember(Name = "dependencies")]
        public string[] Dependencies;

        // exclusive to aki, ignored in EscapeFromTarkov_Data/StreamingAssets/Windows/Windows.json
        [DataMember(Name = "modpath")]
        public string ModPath;

        public BundleItem(string filename, uint crc, string[] dependencies, string modpath = "")
        {
            FileName = filename;
            Crc = crc;
            Dependencies = dependencies;
            ModPath = modpath;
        }
    }
}
