using ComponentAce.Compression.Libs.zlib;
using System.Threading.Tasks;

namespace StayInTarkov.ThirdParty
{
    public enum ZlibCompression
    {
        Store = 0,
        Fastest = 1,
        Fast = 3,
        Normal = 5,
        Ultra = 7,
        Maximum = 9
    }

    /// <summary>
    /// Credit: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Common/Utils/Zlib.cs
    /// Modified by: Paulov & Stay in Tarkov team
    /// </summary>
    public static class Zlib
    {
        // Level | CM/CI FLG
        // ----- | ---------
        // 1     | 78 01
        // 2     | 78 5E
        // 3     | 78 5E
        // 4     | 78 5E
        // 5     | 78 5E
        // 6     | 78 9C
        // 7     | 78 DA
        // 8     | 78 DA
        // 9     | 78 DA

        /// <summary>
        /// Check if the file is ZLib compressed
        /// </summary>
        /// <param name="Data">Data</param>
        /// <returns>If the file is Zlib compressed</returns>
        public static bool IsCompressed(byte[] Data)
        {
            // We need the first two bytes;
            // First byte:  Info (CM/CINFO) Header, should always be 0x78
            // Second byte: Flags (FLG) Header, should define our compression level.

            if (Data == null || Data.Length < 3 || Data[0] != 0x78)
            {
                return false;
            }

            switch (Data[1])
            {
                case 0x01:  // fastest
                case 0x5E:  // low
                case 0x9C:  // normal
                case 0xDA:  // max
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Deflate data.
        /// </summary>

        public static byte[] Compress(string data)
        {
            return SimpleZlib.CompressToBytes(data, 6);
        }

        public static async Task<byte[]> CompressAsync(string data, ZlibCompression level)
        {
            return await Task.Run(() => Compress(data));
        }

        /// <summary>
        /// Inflate data.
        /// </summary>
        public static string Decompress(byte[] data)
        {
            return SimpleZlib.Decompress(data);
        }
    }
}