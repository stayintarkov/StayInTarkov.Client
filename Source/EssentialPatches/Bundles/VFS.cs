using System;
using System.Collections.Generic;
using System.IO;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
namespace SIT.Tarkov.Core
{
    public static class VFS
    {
        public static string Cwd { get; private set; }

        static VFS()
        {
            Cwd = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Combine two filepaths.
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Combines the filepath with the current working directory.
        /// </summary>
        public static string FromCwd(this string filepath)
        {
            return Combine(Cwd, filepath);
        }

        /// <summary>
        /// Get directory path of a filepath.
        /// </summary>
        public static string GetDirectory(this string filepath)
        {
            string value = Path.GetDirectoryName(filepath);
            return (!string.IsNullOrWhiteSpace(value)) ? value : string.Empty;
        }

        /// <summary>
        /// Get file of a filepath
        /// </summary>
        public static string GetFile(this string filepath)
        {
            string value = Path.GetFileName(filepath);
            return (!string.IsNullOrWhiteSpace(value)) ? value : string.Empty;
        }

        /// <summary>
        /// Get file name of a filepath
        /// </summary>
        public static string GetFileName(this string filepath)
        {
            string value = Path.GetFileNameWithoutExtension(filepath);
            return (!string.IsNullOrWhiteSpace(value)) ? value : string.Empty;
        }

        /// <summary>
        /// Get file extension of a filepath.
        /// </summary>
        public static string GetFileExtension(this string filepath)
        {
            string value = Path.GetExtension(filepath);
            return (!string.IsNullOrWhiteSpace(value)) ? value : string.Empty;
        }

        /// <summary>
        /// Move file from one place to another
        /// </summary>
        public static void MoveFile(string a, string b)
        {
            new FileInfo(a).MoveTo(b);
        }

        /// <summary>
        /// Does the filepath exist?
        /// </summary>
        public static bool Exists(string filepath)
        {
            return Directory.Exists(filepath) || File.Exists(filepath);
        }

        /// <summary>
        /// Create directory (recursive).
        /// </summary>
        public static void CreateDirectory(string filepath)
        {
            Directory.CreateDirectory(filepath);
        }

        /// <summary>
        /// Get file content as bytes.
        /// </summary>
        public static byte[] ReadFile(string filepath)
        {
            return File.ReadAllBytes(filepath);
        }

        /// <summary>
        /// Get file content as string.
        /// </summary>
        public static string ReadTextFile(string filepath)
        {
            return File.ReadAllText(filepath);
        }

        /// <summary>
        /// Write data to file.
        /// </summary>
        public static void WriteFile(string filepath, byte[] data)
        {
            if (!Exists(filepath))
            {
                CreateDirectory(filepath.GetDirectory());
            }

            File.WriteAllBytes(filepath, data);
        }

        /// <summary>
        /// Write string to file.
        /// </summary>
        public static void WriteTextFile(string filepath, string data, bool append = false)
        {
            if (!Exists(filepath))
            {
                CreateDirectory(filepath.GetDirectory());
            }

            if (append)
            {
                File.AppendAllText(filepath, data);
            }
            else
            {
                File.WriteAllText(filepath, data);
            }
        }

        /// <summary>
        /// Get directories in directory by full path.
        /// </summary>
        public static string[] GetDirectories(string filepath)
        {
            DirectoryInfo di = new(filepath);
            List<string> paths = new();

            foreach (DirectoryInfo directory in di.GetDirectories())
            {
                paths.Add(directory.FullName);
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Get files in directory by full path.
        /// </summary>
        public static string[] GetFiles(string filepath)
        {
            DirectoryInfo di = new(filepath);
            List<string> paths = new();

            foreach (FileInfo file in di.GetFiles())
            {
                paths.Add(file.FullName);
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Delete directory.
        /// </summary>
        public static void DeleteDirectory(string filepath)
        {
            DirectoryInfo di = new(filepath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            foreach (DirectoryInfo directory in di.GetDirectories())
            {
                DeleteDirectory(directory.FullName);
            }

            di.Delete();
        }

        /// <summary>
        /// Delete file.
        /// </summary>
        public static void DeleteFile(string filepath)
        {
            FileInfo file = new(filepath);
            file.IsReadOnly = false;
            file.Delete();
        }

        /// <summary>
        /// Get files count inside directory recursively
        /// </summary>
        public static int GetFilesCount(string filepath)
        {
            DirectoryInfo di = new(filepath);
            return di.Exists ? di.GetFiles("*.*", SearchOption.AllDirectories).Length : -1;
        }
    }
}
