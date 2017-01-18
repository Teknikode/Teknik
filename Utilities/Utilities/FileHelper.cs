using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public static class FileHelper
    {
        public static string GenerateUniqueFileName(string path, string extension, int length)
        {
            if (Directory.Exists(path))
            {
                string filename = StringHelper.RandomString(length);
                string subDir = filename[0].ToString();
                path = Path.Combine(path, subDir);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                while (File.Exists(Path.Combine(path, string.Format("{0}.{1}", filename, extension))))
                {
                    filename = StringHelper.RandomString(length);
                    subDir = filename[0].ToString();
                    path = Path.Combine(path, subDir);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                return Path.Combine(path, string.Format("{0}.{1}", filename, extension));
            }

            return string.Empty;
        }

        public static string GetDefaultExtension(string mimeType)
        {
            return GetDefaultExtension(mimeType, string.Empty);
        }

        public static string GetDefaultExtension(string mimeType, string defaultExtension)
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : defaultExtension;

            return result;
        }
    }
}
