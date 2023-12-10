using System.IO;
using System.Text;
using ComponentAce.Compression.Libs.zlib;

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
        /// <summary>
        /// Check if the file is ZLib compressed
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>If the file is Zlib compressed</returns>
        public static bool IsCompressed(byte[] data)
        {
            if (data == null || data.Length < 3)
            {
                return false;
            }

            // data[0]: Info (CM/CINFO) Header; must be 0x78
            if (data[0] != 0x78)
            {
                return false;
            }

            // data[1]: Flags (FLG) Header; compression level.
            switch (data[1])
            {
                case 0x01:  // [0x78 0x01] level 0-2: fastest
                case 0x5E:  // [0x78 0x5E] level 3-4: low
                case 0x9C:  // [0x78 0x9C] level 5-6: normal
                case 0xDA:  // [0x78 0xDA] level 7-9: max
                    return true;
            }

            return false;
        }

        private static byte[] Run(byte[] data, ZlibCompression level)
        {
            using (var ms = new MemoryStream())
            {
                using (var zs = (level > ZlibCompression.Store)
                    ? new ZOutputStream(ms, (int)level)
                    : new ZOutputStream(ms))
                {
                    zs.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deflate data.
        /// </summary>
        public static byte[] Compress(byte[] data, ZlibCompression level = ZlibCompression.Normal)
        {
            return Run(data, level);
        }

        /// <summary>
        /// Inflate data.
        /// </summary>
        //public static byte[] Decompress(byte[] data)
        //{
        //    return Run(data, ZlibCompression.Store);
        //}

        public static string Decompress(byte[] data)
        {
            return UTF8Encoding.UTF8.GetString(Run(data, ZlibCompression.Normal));
        }
    }
}