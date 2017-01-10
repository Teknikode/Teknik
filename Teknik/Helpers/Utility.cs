using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Teknik.Utilities;

namespace Teknik.Helpers
{
    public static class Utility
    {
        public static dynamic Merge(object item1, object item2)
        {
            if (item1 == null || item2 == null)
                return item1 ?? item2 ?? new ExpandoObject();

            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;
            foreach (System.Reflection.PropertyInfo fi in item1.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item1, null);
            }
            foreach (System.Reflection.PropertyInfo fi in item2.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item2, null);
            }
            return result;
        }

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

        public static string GetDefaultExtension(string mimeType, string defaultExtension = "")
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : defaultExtension;

            return result;
        }

        public static byte[] ImageToByte(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }
    }
}